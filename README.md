# CadastroDeContatos API

API RESTful de cadastro de usuários desenvolvida em **.NET 10**, seguindo os princípios de **DDD (Domain-Driven Design)**, **SOLID** e boas práticas de arquitetura em camadas.

---

## Arquitetura

```
Contatos.Api            → Controllers, Middlewares, Extensions, Program.cs
Contatos.Domain         → Entities, ValueObjects, Services, Interfaces, DTOs, Validators
Contatos.Infra.Data     → DbContext, Repositories, Configurations, Migrations
Contatos.Tests          → Testes unitários e de integração
```

---

## Tecnologias e Pacotes

| Tecnologia | Uso |
|---|---|
| .NET 10 | Framework principal |
| ASP.NET Core | Web API |
| Entity Framework Core 10 | ORM e Migrations |
| SQL Server | Banco de dados |
| FluentValidation 12 | Validação de requests |
| BCrypt.Net-Next | Hash de senhas |
| JWT Bearer | Autenticação |
| Scalar / Swagger | Documentação da API |
| xUnit | Framework de testes |
| Moq | Mocks para testes unitários |
| Bogus | Geração de dados fake |
| Microsoft.AspNetCore.Mvc.Testing | Testes de integração |

---

## Autenticação

A API utiliza **JWT Bearer**. Todos os endpoints de `/api/usuarios` exigem autenticação.

**Fluxo:**
1. Criar usuário via `POST /api/usuarios/criar`
2. Autenticar via `POST /api/auth/login`
3. Utilizar o token retornado no header: `Authorization: Bearer {token}`

---

## Endpoints

### Auth — `/api/auth`

| Método | Rota | Descrição | Autenticação |
|---|---|---|---|
| `POST` | `/api/auth/login` | Autentica o usuário e retorna o token JWT | Não requerida |

**Body:**
```json
{
  "email": "usuario@email.com",
  "senha": "minhasenha"
}
```

**Resposta `200 OK`:**
```json
{
  "token": "eyJhbGci...",
  "expiracao": "2026-04-17T10:00:00Z"
}
```

---

### Usuários — `/api/usuarios`

Todos os endpoints abaixo exigem `Authorization: Bearer {token}`.

| Método | Rota | Descrição | Status |
|---|---|---|---|
| `POST` | `/api/usuarios/criar` | Cadastra um novo usuário | `201 Created` |
| `GET` | `/api/usuarios/listar` | Lista usuários com paginação | `200 OK` |
| `GET` | `/api/usuarios/obter?email=` | Busca usuário por e-mail | `200 OK` |
| `PUT` | `/api/usuarios/atualizar/{id}` | Atualiza o nome do usuário | `200 OK` |
| `PUT` | `/api/usuarios/atualizar-status/{id}` | Ativa ou inativa o usuário | `200 OK` |

#### POST `/api/usuarios/criar`
```json
{
  "nome": "Fernando Borel",
  "email": "fernando@email.com",
  "senha": "minhasenha"
}
```

#### GET `/api/usuarios/listar`
```
/api/usuarios/listar?pagina=1&tamanhoPagina=10
```
**Resposta:**
```json
{
  "data": [],
  "total": 30,
  "pagina": 1,
  "tamanhoPagina": 10,
  "totalPaginas": 3
}
```
> O tamanho máximo por página é **25**, limitado pelo repositório base.

#### PUT `/api/usuarios/atualizar/{id}`
```json
{ "nome": "Novo Nome" }
```

#### PUT `/api/usuarios/atualizar-status/{id}`
```json
{ "ativo": false }
```

---

## Validações (FluentValidation)

| Campo | Regra |
|---|---|
| `Nome` | Obrigatório, mínimo de 4 caracteres |
| `Email` | Obrigatório, formato válido, único no banco |
| `Senha` | Obrigatória, mínimo de 6 caracteres |

Erros de validação retornam `400 Bad Request`.

---

## Mapeamento de Erros

| Exceção | Status HTTP |
|---|---|
| `ValidationException` (FluentValidation) | `400 Bad Request` |
| `ArgumentException` | `400 Bad Request` |
| `ApplicationException` | `422 Unprocessable Entity` |
| Outros | `500 Internal Server Error` |

---

## Banco de Dados

### Configuração

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=localhost,1434;Initial Catalog=CadastroContatosDB;..."
}
```

| Ambiente | Banco |
|---|---|
| Desenvolvimento | `CadastroContatosDB` |
| Testes | `CadastroContatosDB_Tests` |

### Migrations

```powershell
# Criar migration
dotnet ef migrations add <NomeDaMigration> --project Contatos.Infra.Data --startup-project Contatos.Api

# Aplicar ao banco
dotnet ef database update --project Contatos.Infra.Data --startup-project Contatos.Api
```

---

## Testes

### Unitários — `Contatos.Tests/Services`

Testam a camada de domínio com mocks via Moq e dados gerados pelo Bogus, sem dependência de banco ou infraestrutura.

| Classe | Cenários cobertos |
|---|---|
| `Email` (ValueObject) | Formato válido, nulo, vazio, inválido, `ToString()` |
| `UsuarioService.CriarAsync` | Dados válidos, nome inválido, e-mail inválido, e-mail duplicado |
| `UsuarioService.ObterAsync` | Usuário encontrado, não encontrado |
| `UsuarioService.AtualizarAsync` | Dados válidos, nome inválido, id inexistente |
| `UsuarioService.AtualizarStatusAsync` | Ativar, inativar, id inexistente |
| `UsuarioService.ObterTodosAsync` | Paginação, banco vazio, cálculo de total de páginas, parâmetros inválidos |

### Integração — `Contatos.Tests/Integration`

Sobem a aplicação completa via `WebApplicationFactory` com banco real (`CadastroContatosDB_Tests`). O banco é limpo antes de cada teste via `IAsyncLifetime`. A autenticação é tratada por um `TestAuthHandler` que autentica automaticamente todas as requisições no ambiente `Testing`.

| Endpoint | Cenários cobertos |
|---|---|
| `POST /api/usuarios/criar` | Dados válidos, nome inválido, e-mail inválido, e-mail duplicado |
| `GET /api/usuarios/obter?email=` | Usuário encontrado, não encontrado |
| `GET /api/usuarios/listar` | Com usuários, banco vazio, paginação, parâmetros inválidos |
| `PUT /api/usuarios/atualizar/{id}` | Dados válidos, nome inválido, id inexistente |
| `PUT /api/usuarios/atualizar-status/{id}` | Inativar, reativar, id inexistente |

```powershell
dotnet test
```

---

## Como executar

```powershell
# Restaurar pacotes
dotnet restore

# Aplicar migrations
dotnet ef database update --project Contatos.Infra.Data --startup-project Contatos.Api

# Executar a API
dotnet run --project Contatos.Api
```

Documentação disponível em:
- **Swagger:** `https://localhost:{porta}/swagger`
- **Scalar:** `https://localhost:{porta}/scalar/v1`
