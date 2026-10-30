// Fumaça de ponta a ponta contra a API rodando: cobre o fluxo completo da
// chamada (abrir → retirada → fechar → relatório), o isolamento entre
// tenants e o consentimento LGPD (conceder/revogar). Sem framework de teste —
// assert.strict e um corpo de saída não-zero bastam para um script e verde na CI.
// Uso: node scripts/e2e.mjs (API precisa estar rodando — ver README).
import assert from 'node:assert/strict';
import { get, post, put, unique, ApiError, API_URL } from './lib/client.mjs';

const todayIso = () => new Date().toISOString().slice(0, 10);

const registerTransporter = async () => {
  const email = unique('e2e.transportador') + '@example.com';
  const response = await post('/api/auth/register', {
    name: 'Transportador E2E',
    email,
    phone: '11999990000',
    password: 'Demo12345',
    document_type: 'Cpf',
    document_number: '98765432100',
  });
  return { token: response.token, email };
};

const login = async (email, password) => (await post('/api/auth/login', { email, password })).token;

const expectError = async (promise, expectedStatus, label) => {
  try {
    await promise;
    throw new Error(`${label}: esperava HTTP ${expectedStatus}, mas a chamada teve sucesso`);
  } catch (error) {
    if (!(error instanceof ApiError)) throw error;
    assert.equal(error.status, expectedStatus, `${label}: status esperado ${expectedStatus}, veio ${error.status}`);
  }
};

const run = async () => {
  console.log(`Rodando e2e contra ${API_URL}...`);
  const sessionDate = todayIso();

  // --- Setup: transportador, escola, grupo, aluno matriculado, responsável com permissão de retirada.
  const { token } = await registerTransporter();

  const school = await post('/api/schools', { name: 'Escola E2E', address: null }, token);
  const group = await post('/api/transport-groups', { name: 'Grupo E2E', shift: 'Manhã' }, token);
  const student = await post(
    '/api/students',
    { name: 'Aluno E2E', birth_date: '2015-01-01', grade: '5°A', school_id: school.id },
    token
  );
  await post(`/api/students/${student.id}/enrollments`, { transport_group_id: group.id }, token);

  const guardianPassword = 'Demo12345';
  const guardianEmail = unique('e2e.responsavel') + '@example.com';
  const guardian = await post(
    '/api/guardians',
    { name: 'Responsável E2E', email: guardianEmail, password: guardianPassword, contact: null },
    token
  );
  await post(
    `/api/students/${student.id}/guardians`,
    { guardian_id: guardian.id, relationship: 'Father', is_primary: true, can_pickup: true },
    token
  );

  const otherGuardianEmail = unique('e2e.outro-responsavel') + '@example.com';
  const otherGuardian = await post(
    '/api/guardians',
    { name: 'Outro Responsável E2E', email: otherGuardianEmail, password: guardianPassword, contact: null },
    token
  );
  // otherGuardian não é vinculado ao aluno — deve ser recusado ao tentar retirar.

  // --- 3. Abrir a chamada gera uma linha Present por matrícula ativa.
  const session = await post(
    `/api/transport-groups/${group.id}/attendance-sessions`,
    { session_type: 'ToSchool', session_date: sessionDate, vehicle_id: null, assistant_id: null },
    token
  );
  assert.equal(session.records.length, 1, 'roster deveria ter 1 aluno matriculado');
  assert.equal(session.records[0].status, 'Present', 'aluno deveria nascer Present');
  console.log('  ok: abrir sessão cria roster Present');

  // --- 4. Segunda sessão no mesmo grupo/data/sentido conflita.
  await expectError(
    post(
      `/api/transport-groups/${group.id}/attendance-sessions`,
      { session_type: 'ToSchool', session_date: sessionDate, vehicle_id: null, assistant_id: null },
      token
    ),
    409,
    'sessão duplicada'
  );
  console.log('  ok: segunda sessão no mesmo grupo/data/sentido conflita (409)');

  // --- 1. Retirada sem can_pickup é recusada.
  await expectError(
    post(
      `/api/attendance-sessions/${session.id}/records/${student.id}/pickup`,
      { guardian_id: otherGuardian.id, justification: null },
      token
    ),
    400,
    'retirada sem can_pickup'
  );
  console.log('  ok: retirada sem can_pickup é recusada (400)');

  // --- 2. Retirada com can_pickup vira PickedUpByGuardian.
  await post(
    `/api/attendance-sessions/${session.id}/records/${student.id}/pickup`,
    { guardian_id: guardian.id, justification: 'Buscado pela manhã' },
    token
  );
  const sessionAfterPickup = await get(`/api/attendance-sessions/${session.id}`, token);
  assert.equal(sessionAfterPickup.records[0].status, 'PickedUpByGuardian', 'status deveria ser PickedUpByGuardian');
  assert.equal(sessionAfterPickup.records[0].picked_up_by_guardian_id, guardian.id);
  console.log('  ok: retirada com can_pickup marca PickedUpByGuardian');

  await post(`/api/attendance-sessions/${session.id}/close`, undefined, token);

  // --- 5. Serviço chamado com o TenantId de outro transportador retorna NotFound.
  const { token: otherTransporterToken } = await registerTransporter();
  await expectError(get(`/api/transport-groups/${group.id}`, otherTransporterToken), 404, 'isolamento entre tenants');
  console.log('  ok: outro tenant recebe 404 ao acessar o grupo');

  // --- 6. Consentimento: conceder e revogar.
  const guardianToken = await login(guardianEmail, guardianPassword);

  const beforeConsent = await get('/api/me/consents', guardianToken);
  assert.equal(beforeConsent.find((c) => c.student_id === student.id).active, false, 'consentimento deveria nascer inativo');

  const granted = await post(`/api/me/children/${student.id}/consent`, { terms_version: '1.0' }, guardianToken);
  assert.equal(granted.active, true, 'consentimento deveria ficar ativo após conceder');

  await expectError(
    post(`/api/me/children/${student.id}/consent`, { terms_version: '1.0' }, guardianToken),
    409,
    'conceder consentimento já ativo'
  );

  await post(`/api/me/children/${student.id}/consent/revoke`, undefined, guardianToken);
  const afterRevoke = await get('/api/me/consents', guardianToken);
  assert.equal(afterRevoke.find((c) => c.student_id === student.id).active, false, 'consentimento deveria ficar inativo após revogar');

  await expectError(
    post(`/api/me/children/${student.id}/consent/revoke`, undefined, guardianToken),
    409,
    'revogar consentimento já revogado'
  );
  console.log('  ok: consentimento concede e revoga, com histórico preservado');

  console.log('\nTudo passou.');
};

run().catch((error) => {
  console.error('\nFalhou:', error);
  process.exitCode = 1;
});
