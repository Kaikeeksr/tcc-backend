# API de Gestão de Chamadas — Transporte Escolar

API .NET 10 para gestão de chamadas de transportadores escolares (os "tios da van"), em arquitetura de 4 camadas com PostgreSQL, OpenAPI 3.1 e interface Scalar.

> **Estado atual:** modelo de dados aplicado no banco e **superfície da API construída** — 60 operações em 14 controllers, cobrindo autenticação, cadastros, chamada (incluindo a retirada pelo responsável), calendário letivo e relatórios de frequência. O app cliente ([`tcc-mobie-app`](https://github.com/Kaikeeksr/tcc-mobie-app)) consome esta API sem nenhum dado mockado. Ver [O que ainda falta](#o-que-ainda-falta).

---

## Como rodar

Pré-requisito: **.NET SDK 10.0.302+** (`dotnet --version`).

```bash
dotnet build
```

```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=<host>;Port=5432;Database=<db>;Username=<user>;Password=<senha>;SSL Mode=Require;Trust Server Certificate=true;Maximum Pool Size=4" --project src/AttendanceManagement.Api
```

```bash
dotnet user-secrets set "Jwt:Key" "<chave-de-no-minimo-32-bytes>" --project src/AttendanceManagement.Api
```

```bash
dotnet ef database update --project src/AttendanceManagement.Infrastructure --startup-project src/AttendanceManagement.Api
```

```bash
dotnet run --project src/AttendanceManagement.Api
```

Abre em **<http://localhost:5218/scalar>**.

Se `dotnet ef` não existir: `dotnet tool install --global dotnet-ef`.

---

## Endpoints

60 operações. Todas exigem `Authorization: Bearer <token>` e são escopadas pelo tenant, exceto `register` e `login`. A documentação viva está em `/scalar`; a tabela abaixo é o mapa.

### Infraestrutura

| Método | Rota | O que faz |
|---|---|---|
| `GET` | `/health` | Liveness — o processo está de pé. Não toca no banco. |
| `GET` | `/health/ready` | Readiness — consulta o PostgreSQL. |
| `GET` | `/scalar` | Documentação interativa (fora de Produção). |
| `GET` | `/openapi/v1.json` | Documento OpenAPI 3.1. |

### Autenticação — `/api/auth` (3)

| Método | Rota | O que faz |
|---|---|---|
| `POST` | `/api/auth/register` | Autocadastro do transportador (CPF ou CNPJ). Devolve token. |
| `POST` | `/api/auth/login` | Login de qualquer perfil. Devolve token e o usuário autenticado. |
| `GET` | `/api/auth/me` | Quem é o portador do token. |

### Cadastros (35)

| Recurso | Rotas | Operações |
|---|---|---|
| Veículos | `/api/vehicles` | listar, obter, criar, atualizar, remover |
| Escolas | `/api/schools` | listar, obter, criar, atualizar, remover |
| Monitores | `/api/assistants` | listar, obter, criar, atualizar, remover |
| Responsáveis | `/api/guardians` | listar, obter, criar, atualizar, remover |
| Alunos | `/api/students` | listar, obter, criar, atualizar, remover, criar login (`POST /{id}/login`) |
| Grupos de transporte | `/api/transport-groups` | listar, obter, criar, atualizar, remover, designar tripulação (`PUT /{id}/crew`) |
| Vínculo responsável ↔ aluno | `/api/students/{id}/guardians`, `/api/guardian-students/{id}` | listar, vincular, atualizar (parentesco, contato principal, permissão de retirada), desvincular |
| Matrículas | `/api/students/{id}/enrollments`, `/api/enrollments/{id}` | listar, matricular, encerrar |
| Equipe | `/api/transporters/me/team` | monitores do transportador |

### Chamada — 11 operações

| Método | Rota | O que faz |
|---|---|---|
| `POST` | `/api/transport-groups/{groupId}/attendance-sessions` | Abre a chamada do dia (ida ou volta). O roster nasce dos matriculados ativos, **já como presentes**. |
| `GET` | `/api/transport-groups/{groupId}/attendance-sessions/by-date` | Busca a sessão de uma data e sentido. |
| `GET` | `/api/transport-groups/{groupId}/attendance-sessions` | Histórico de sessões do grupo num intervalo. |
| `GET` | `/api/attendance-sessions/{id}` | Sessão pelo id, com o roster. |
| `PUT` | `/api/attendance-sessions/{id}/records` | Marca presença/falta/atraso/justificado em lote. |
| `POST` | `/api/attendance-sessions/{id}/records/{studentId}/pickup` | Registra retirada pelo responsável (valida `can_pickup`). |
| `PUT` | `/api/attendance-sessions/{id}/records/{studentId}/justify` | Justifica um registro. |
| `POST` | `/api/attendance-sessions/{id}/close` | Fecha a chamada — não admite mais edição. |
| `POST` | `/api/attendance-sessions/{id}/cancel` | Cancela a chamada aberta por engano. |
| `GET` | `/api/transport-groups/{groupId}/attendance-report` | Frequência do grupo, agregada por aluno, num intervalo. |
| `GET` | `/api/students/{studentId}/attendance-history` | Histórico e agregado de um aluno. |

### Calendário letivo — `/api/calendar-days` (3)

Só o **desvio** é gravado: toda data é letiva por omissão, fins de semana são não letivos por regra, e apenas feriados e recessos entram na tabela.

| Método | Rota | O que faz |
|---|---|---|
| `GET` | `/api/calendar-days?from=&to=` | Dias marcados no intervalo. |
| `PUT` | `/api/calendar-days` | Marca uma data (ou período) como feriado/letivo. |
| `DELETE` | `/api/calendar-days/{date}` | Remove a marcação. |

### Perfis responsável e aluno — `/api/me` (3)

Escopo restrito de propósito: devolve **apenas** os alunos vinculados a quem pede.

| Método | Rota | O que faz |
|---|---|---|
| `GET` | `/api/me/children` | Filhos do responsável autenticado. |
| `GET` | `/api/me/children/{studentId}/attendance` | Frequência de um filho. |
| `GET` | `/api/me/attendance` | Frequência do próprio aluno autenticado. |

---

## Arquitetura

```
Domain  ←  Application  ←  Infrastructure
   ↑            ↑               ↑
   └────────── Api ─────────────┘
```

Dependências verificadas pelo compilador, não por convenção.

| Projeto | Responsabilidade | Pacotes NuGet |
|---|---|---|
| **Domain** | Entidades, regras de negócio, `Result`/`Error`. C# puro. | **nenhum** |
| **Application** | Casos de uso, contratos de persistência, validadores. | 2 (só abstrações de DI e FluentValidation) |
| **Infrastructure** | EF Core, Npgsql, configurações, migrations, JWT, hashing. | 7 |
| **Api** | Pipeline HTTP, composition root, controllers. | 5 (uma delas, `EFCore.Design`, só em tempo de projeto) |

O que torna a API agnóstica de banco: as interfaces de persistência são declaradas na Application e implementadas na Infrastructure. O `Program.cs` chama `AddInfrastructure()` em **uma linha** — é o único ponto do sistema que sabe que PostgreSQL existe.

---

## Autenticação e isolamento entre tenants

- **Senha:** PBKDF2 sobre HMAC-SHA256, 600.000 iterações, salt de 16 bytes por credencial. O valor gravado carrega iterações + salt + hash, então o custo pode subir sem invalidar as credenciais existentes. Comparação em tempo constante.
- **Token:** JWT (RFC 7519) assinado em HMAC-SHA256, com validação de issuer, audience, assinatura e validade. Sete claims, entre elas `transporter_id` e `profile_id`. Nenhum dado pessoal sensível vai no payload — o conteúdo do JWT é codificado, não cifrado (RFC 8725).
- **Isolamento:** o `transporter_id` é lido **exclusivamente** da claim, nunca do corpo ou da rota, e num único ponto: o `ApiController` base do qual todos os controllers derivam. Recurso de outro tenant responde **404**, não 403 — 403 já revelaria que ele existe.

---

## Modelo de dados

14 tabelas, nomenclatura em inglês, snake_case no banco.

**Identidade**
- `user_account` — credencial única de login. Não guarda dado de negócio.

**Perfis** (escopados por transportador)
- `transporter` — o "tio da van". **Raiz do tenant**: todo dado pertence a um. Tem CPF/CNPJ e CNH.
- `assistant` — o monitor. Login próprio, pertence a exatamente um transportador.
- `guardian` — o responsável. Concentra os dados de contato, e por isso é o centro da estratégia de LGPD. Login obrigatório.
- `student` — o aluno. Login opcional. Tem `grade` (série escolar, ex.: `"3°A"`).

**Operação**
- `vehicle`, `school`, `transport_group` (o grupo da van, com veículo e monitor designados)
- `calendar_day` — feriados e recessos do transportador. Só o desvio é gravado.

**Relacionamentos**
- `guardian_student` — N:N com parentesco, contato principal e permissão de retirada
- `enrollment` — N:N aluno ↔ grupo, temporal (troca de grupo preserva histórico)

**Chamada e histórico**
- `attendance_session` — a chamada (ida/volta, por grupo, por dia)
- `attendance_record` — a linha do aluno na chamada
- `event_log` — trilha append-only de eventos, permanente. Escrita pelo `AttendanceService` na abertura, na retirada e no encerramento da chamada.

### Dois conceitos que a palavra "turma" confunde

| Português | No modelo | O que é |
|---|---|---|
| turma (da van) | `transport_group` | grupo operacional, com veículo e monitor |
| turma (escolar) | `student.grade` | série, ex.: `"3°A"` — texto livre |

### O caso central: "retirado pelo responsável"

É o caso que orientou boa parte do modelo. Se o responsável busca o aluno na escola antes da van chegar, isso **não é falta**. O `attendance_record` tem colunas tipadas para o caso: status `PickedUpByGuardian`, quem retirou, justificativa e hora. Antes de aceitar, o serviço confere o `guardian_student.can_pickup` daquele responsável sobre aquele aluno — sem permissão, a operação é recusada. Um índice parcial faz a tela "quem foi retirado hoje" varrer só as linhas retiradas.

### Tratamento de erro

Regra de negócio violada **não lança exceção**: as entidades devolvem `Result<T>`, e a camada de API traduz o `ErrorType` em status HTTP (`Validation`→400, `Unauthorized`→401, `NotFound`→404, `Conflict`→409). Exceção fica para o inesperado, capturada pelo `GlobalExceptionHandler` como `ProblemDetails` (RFC 9457) com `errorCode` estável e `traceId`.

---

## Decisões de projeto

| O quê | Por quê |
|---|---|
| UUID v7 nos IDs | sequenciais no tempo — evitam a fragmentação de índice do `Guid.NewGuid()` |
| `transporter_id` denormalizado nas tabelas quentes | filtro de tenant vira índice, sem join |
| Snapshots (veículo/monitor na sessão, escola no record) | o histórico não muda se a designação mudar depois |
| Roster criado já como presente na abertura da sessão | o esforço do condutor passa a ser proporcional às exceções, não ao tamanho do grupo |
| Soft delete (`deleted_at_utc`) + filtro global do EF | sumir das listas sem perder integridade referencial |
| Enums gravados como texto | consultar no psql/DBeaver fica legível |
| Migrations explícitas (sem `Database.Migrate()` no boot) | duas instâncias subindo juntas brigariam pelo schema |
| Compressão Brotli/Gzip, `AddDbContextPool`, `NoTracking` global | throughput |

Sem AutoMapper e sem MediatR — ambos passaram a exigir licença comercial paga, e não valia prender o projeto nisso.

**LGPD:** o dado pessoal fica centralizado no `guardian`, então a anonimização de erasure toca poucas tabelas. Soft delete **não** é erasure — são operações distintas.

---

## Banco (Clever Cloud)

Dois detalhes que separam "conecta" de "não conecta":

- **`Trust Server Certificate=true`** é obrigatório junto com `SSL Mode=Require`: a partir do Npgsql 8, `Require` deixou de implicar confiança no certificado.
- **`Maximum Pool Size` baixo (4)**: planos menores limitam conexões simultâneas; pool grande demais vira erro, não performance.

Em deploy, as variáveis `ConnectionStrings__DefaultConnection` e `Jwt__Key` sobrepõem o user-secrets.

> A connection string e a chave do JWT **nunca** vão para o repositório — vivem em `dotnet user-secrets` (fora da pasta do projeto) ou em variável de ambiente.

---

## O que ainda falta

1. **Testes** — xUnit no Domain (que não tem dependência nenhuma, então testa fácil) e Testcontainers na Infrastructure.
2. **Consentimento LGPD** — o titular é menor, então o consentimento do responsável (art. 14, §1º da Lei nº 13.709/2018) precisa ser registrável, datável e revogável.
3. **Paginação** em toda listagem, antes de qualquer ideia de produção.
4. **Publicação** da API em ambiente de produção. O banco já está em nuvem.
5. **Exportação em planilha** (XLSX) dos relatórios de frequência — hoje só há PDF, gerado no cliente.

---

## Sobre

Backend do Trabalho de Conclusão de Curso em Engenharia de Software. O cliente que o consome está em [`tcc-mobie-app`](https://github.com/Kaikeeksr/tcc-mobie-app).

**Kaike Santos Rocha** — [@Kaikeeksr](https://github.com/Kaikeeksr)
