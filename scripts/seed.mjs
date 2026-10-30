// Povoa a API com um cenário de demonstração: um transportador, uma escola, um
// grupo de transporte, dois alunos matriculados e um responsável com permissão
// de retirada. Uso: node scripts/seed.mjs (API precisa estar rodando — ver README).
import { post, put, unique, API_URL } from './lib/client.mjs';

const run = async () => {
  console.log(`Semeando ${API_URL}...`);

  const transporter = await post('/api/auth/register', {
    name: 'Transportador Demo',
    email: unique('transportador.demo') + '@example.com',
    phone: '11999990000',
    password: 'Demo12345',
    document_type: 'Cpf',
    document_number: '12345678901',
  });
  const token = transporter.token;
  console.log(`Transportador criado: ${transporter.user.email}`);

  const school = await post('/api/schools', { name: 'Escola Demo', address: null }, token);

  const vehicle = await post('/api/vehicles', { plate: 'DEM0123', model: 'Sprinter', capacity: 15 }, token);

  const group = await post('/api/transport-groups', { name: 'Van Demo - Manhã', shift: 'Manhã' }, token);
  await put(`/api/transport-groups/${group.id}/crew`, { vehicle_id: vehicle.id, assistant_id: null }, token);

  const studentA = await post(
    '/api/students',
    { name: 'Aluno Demo A', birth_date: '2015-03-10', grade: '3°A', school_id: school.id },
    token
  );
  const studentB = await post(
    '/api/students',
    { name: 'Aluno Demo B', birth_date: '2014-07-22', grade: '4°B', school_id: school.id },
    token
  );

  await post(`/api/students/${studentA.id}/enrollments`, { transport_group_id: group.id }, token);
  await post(`/api/students/${studentB.id}/enrollments`, { transport_group_id: group.id }, token);

  const guardianEmail = unique('responsavel.demo') + '@example.com';
  const guardian = await post(
    '/api/guardians',
    { name: 'Responsável Demo', email: guardianEmail, password: 'Demo12345', contact: { mobile: '11988880000' } },
    token
  );

  await post(
    `/api/students/${studentA.id}/guardians`,
    { guardian_id: guardian.id, relationship: 'Father', is_primary: true, can_pickup: true },
    token
  );

  console.log('\nCenário criado:');
  console.log(`  Transportador: ${transporter.user.email} / Demo12345`);
  console.log(`  Responsável:   ${guardianEmail} / Demo12345`);
  console.log(`  Escola:        ${school.id}`);
  console.log(`  Grupo:         ${group.id}`);
  console.log(`  Alunos:        ${studentA.id}, ${studentB.id}`);
};

run().catch((error) => {
  console.error('Falha ao semear:', error);
  process.exitCode = 1;
});
