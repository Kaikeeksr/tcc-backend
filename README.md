# API de Gestão de Chamadas — Transporte Escolar

API .NET 10 para gestão de chamadas de transportadores escolares ("tios da van"), em arquitetura de 4 camadas com PostgreSQL, OpenAPI 3.1 e interface Scalar.

> **Estado atual:** o modelo de dados está completo e aplicado no banco. A superfície da API (endpoints de domínio) **ainda não foi construída** — hoje a aplicação sobe com health checks e a documentação. Ver [Próximos passos](#próximos-passos).

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
dotnet ef database update --project src/AttendanceManagement.Infrastructure --startup-project src/AttendanceManagement.Api
```

```bash
dotnet run --project src/AttendanceManagement.Api
```

Abre em **<http://localhost:5218/scalar>**.

Se `dotnet ef` não existir: `dotnet tool install --global dotnet-ef`.

### Endpoints disponíveis

| Método | Rota | O que faz |
|---|---|---|
| `GET` | `/health` | Liveness — o processo está de pé. Não toca no banco. |
| `GET` | `/health/ready` | Readiness — consulta o PostgreSQL. |
| `GET` | `/scalar` | Documentação interativa. |
| `GET` | `/openapi/v1.json` | Documento OpenAPI 3.1. |

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
| **Application** | Casos de uso e contratos de persistência. | 1 (só abstrações de DI) |
| **Infrastructure** | EF Core, Npgsql, configurações, migrations. | 6 |
| **Api** | Pipeline HTTP, composition root. | 3 |

O que torna a API agnóstica de banco: as interfaces de persistência são declaradas na Application e implementadas na Infrastructure. O `Program.cs` chama `AddInfrastructure()` em **uma linha** — é o único ponto que sabe que PostgreSQL existe.

---

## Modelo de dados

13 tabelas, nomenclatura em inglês, snake_case no banco.

**Identidade**
- `user_account` — credencial única de login. Não guarda dado de negócio.

**Perfis** (escopados por transportador)
- `transporter` — o "tio da van". **Raiz do tenant**: todo dado pertence a um. Tem CPF/CNPJ e CNH.
- `assistant` — o monitor. Login próprio, pertence a exatamente um transportador.
- `guardian` — o responsável. Concentra os dados de contato (centro da LGPD). Login obrigatório.
- `student` — o aluno. Login opcional. Tem `grade` (série escolar, ex.: `"3°A"`).

**Operação**
- `vehicle`, `school`, `transport_group` (o grupo da van, com veículo e monitor designados)

**Relacionamentos**
- `guardian_student` — N:N com parentesco, contato principal e permissão de retirada
- `enrollment` — N:N aluno ↔ grupo, temporal (troca de grupo preserva histórico)

**Chamada e histórico**
- `attendance_session` — a chamada (ida/volta, por grupo, por dia)
- `attendance_record` — a linha do aluno na chamada
- `event_log` — trilha append-only de eventos, permanente

### Dois conceitos que a palavra "turma" confunde

| Português | No modelo | O que é |
|---|---|---|
| turma (da van) | `transport_group` | grupo operacional, com veículo e monitor |
| turma (escolar) | `student.grade` | série, ex.: `"3°A"` — texto livre |

### O caso central: "retirado pelo responsável"

Se o responsável busca o aluno na escola antes da van chegar, isso **não é falta**. O `attendance_record` tem colunas tipadas para o caso: status `PickedUpByGuardian`, quem retirou, justificativa e hora. Um índice parcial faz a tela "quem foi retirado hoje" varrer só as linhas retiradas.

### Tratamento de erro

Regra de negócio violada **não lança exceção**: as entidades devolvem `Result<T>`, e a camada de API traduz o `ErrorType` em status HTTP (`Validation`→400, `NotFound`→404, `Conflict`→409). Exceção fica para o inesperado, capturada pelo `GlobalExceptionHandler` como `ProblemDetails` (RFC 9457) com `traceId`.

---

## Decisões de projeto

| O quê | Por quê |
|---|---|
| UUID v7 nos IDs | sequenciais no tempo — evitam a fragmentação de índice do `Guid.NewGuid()` |
| `transporter_id` denormalizado nas tabelas quentes | filtro de tenant vira índice, sem join |
| Snapshots (veículo/monitor na sessão, escola no record) | o histórico não muda se a designação mudar depois |
| Soft delete (`deleted_at_utc`) + filtro global do EF | sumir das listas sem perder integridade referencial |
| Enums gravados como texto | consultar no psql/DBeaver fica legível |
| Migrations explícitas (sem `Database.Migrate()` no boot) | duas instâncias subindo juntas brigariam pelo schema |
| Compressão Brotli/Gzip, `AddDbContextPool`, `NoTracking` global | throughput |

Sem AutoMapper e sem MediatR — ambos passaram a exigir licença comercial paga.

**LGPD:** dado pessoal fica centralizado no `guardian`, então a anonimização de erasure toca poucas tabelas. Soft delete **não** é erasure — são operações distintas.

---

## Banco (Clever Cloud)

Dois detalhes que separam "conecta" de "não conecta":

- **`Trust Server Certificate=true`** é obrigatório junto com `SSL Mode=Require`: a partir do Npgsql 8, `Require` deixou de implicar confiança no certificado.
- **`Maximum Pool Size` baixo (4)**: planos menores limitam conexões simultâneas; pool grande demais vira erro, não performance.

Em deploy, a variável `ConnectionStrings__DefaultConnection` sobrepõe o user-secrets.

> A connection string **nunca** vai para o repositório — vive em `dotnet user-secrets` (fora da pasta do projeto) ou em variável de ambiente.

---

## Próximos passos

1. **Superfície da API** — services, DTOs, repositórios e controllers. O fluxo natural para começar: cadastro (transportador → veículo → grupo → aluno + responsável) → abrir chamada → marcar "retirado pelo responsável".
2. **Autenticação** (JWT) — as tabelas de identidade existem, mas nada está plugado.
3. **Paginação** em toda listagem antes de ir a produção.
4. **Testes** — xUnit no Domain (não tem dependência, testa fácil) e Testcontainers na Infrastructure.
5. **Consentimento LGPD** — o titular é menor; o consentimento do responsável precisa ser registrável e datável.
