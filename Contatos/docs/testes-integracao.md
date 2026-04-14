# Testes de Integração

## Visão geral

Os testes de integração sobem a aplicação inteira usando `WebApplicationFactory` e fazem chamadas HTTP reais contra os endpoints. A diferença principal em relação aos unitários é que aqui o banco de dados entra de verdade — sem mocks, sem InMemory.

**Bibliotecas utilizadas:**
- `xUnit` — framework de testes
- `Microsoft.AspNetCore.Mvc.Testing` — responsável por subir o host da aplicação
- `Bogus` — geração de dados fake em pt-BR para evitar colisões entre testes

---

## Configuração do ambiente

### Banco de dados

Para não misturar dados de desenvolvimento com dados de teste, foi criado um arquivo `appsettings.Testing.json` no projeto da API. Ele aponta para um banco separado no mesmo servidor:

```
Desenvolvimento → CadastroContatosDB
Testes          → CadastroContatosDB_Tests
```

O `WebApiFactory` define o ambiente como `"Testing"` no `ConfigureWebHost`, o que faz o ASP.NET Core carregar automaticamente esse arquivo de configuração:

```csharp
builder.UseEnvironment("Testing");
```

### WebApiFactory

**Arquivo:** `Contatos.Tests/Integration/WebApiFactory.cs`

Herda de `WebApplicationFactory<Program>` — isso só funciona porque `Program` foi exposto como `public partial class Program {}` no final do `Program.cs`.

O método `LimparBancoDeDadosAsync` é chamado antes de cada teste. Ele faz duas coisas:

1. `MigrateAsync()` — garante que o schema existe (idempotente, não faz nada se já estiver atualizado)
2. `ExecuteDeleteAsync()` — executa um `DELETE FROM Usuario` direto no banco, sem carregar nada na memória

```csharp
public async Task LimparBancoDeDadosAsync()
{
    using var scope = Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await context.Database.MigrateAsync();
    await context.Set<Usuario>().ExecuteDeleteAsync();
}
```

### Isolamento entre testes

A classe de teste implementa `IAsyncLifetime`. O `InitializeAsync` roda antes de cada método de teste individual — não uma vez por classe, mas uma vez por teste. Isso garante que cada teste começa com o banco limpo:

```
[Antes do Teste A] → MigrateAsync + DELETE
[Teste A executa]  → INSERT / SELECT reais no SQL Server
[Antes do Teste B] → MigrateAsync + DELETE
[Teste B executa]  → banco limpo novamente
```

O `DisposeAsync` não faz nada porque a limpeza já acontece no começo do próximo teste.

---

## UsuariosController — Testes de Integração

**Arquivo:** `Contatos.Tests/Integration/UsuariosControllerIntegrationTests.cs`

O `HttpClient` vem do `factory.CreateClient()` e o `Faker` é inicializado com `pt_BR` para gerar nomes e e-mails realistas. Como cada teste recomeça com banco limpo, não há risco de colisão de e-mails entre testes diferentes.

---

### POST `/api/usuarios/criar`

| Método | Status esperado | Cenário |
|---|---|---|
| `Criar_QuandoDadosValidos_DeveRetornar201ComCorpoCorreto` | `201 Created` | Payload válido, verifica corpo completo da resposta |
| `Criar_QuandoNomeInvalido_DeveRetornar400` | `400 Bad Request` | Nome com menos de 6 caracteres (`"abc"`) |
| `Criar_QuandoEmailInvalido_DeveRetornar400` | `400 Bad Request` | Email sem formato válido (`"email_invalido"`) |
| `Criar_QuandoEmailJaCadastrado_DeveRetornar422` | `422 Unprocessable Entity` | Duas requisições com o mesmo e-mail |

O teste de `201` verifica mais do que o status code — ele desserializa o corpo e confere todos os campos:

```csharp
Assert.NotEqual(Guid.Empty, body.Id);
Assert.Equal(corpo.Nome, body.Nome);
Assert.Equal(corpo.Email.ToLower(), body.Email);  // email sempre em minúsculo
Assert.True(body.Ativo);
```

O `422` é intencional: a regra de negócio de e-mail duplicado lança `ApplicationException`, que o `ErrorHandlingMiddleware` mapeia para esse status.

---

### GET `/api/usuarios/obter/{email}`

| Método | Status esperado | Cenário |
|---|---|---|
| `Obter_QuandoUsuarioExiste_DeveRetornar200ComCorpoCorreto` | `200 OK` | Cria via POST primeiro, depois busca pelo e-mail |
| `Obter_QuandoUsuarioNaoExiste_DeveRetornar422` | `422 Unprocessable Entity` | Busca por e-mail que não existe |

O teste de `200` tem uma dependência intencional de outra rota: ele faz um POST antes do GET para garantir que o usuário existe. Isso é natural em testes de integração — o fluxo reflete o uso real da API.

---

## Observações

**Por que SQL Server real e não InMemory?**
O InMemory não aplica constraints, não executa SQL, e ignora configurações relacionais (índices únicos, `GETDATE()`, etc.). Com o banco real, o teste exercita a query gerada pelo EF Core, o índice único no e-mail, e o comportamento do `SaveChanges` de verdade.

**Por que banco separado e não o de desenvolvimento?**
Porque `LimparBancoDeDadosAsync` executa `DELETE FROM Usuario` sem filtro nenhum. Rodar isso no banco de desenvolvimento apagaria dados reais.

**O que acontece na primeira execução?**
O `MigrateAsync` cria o banco `CadastroContatosDB_Tests` automaticamente (se o usuário `sa` tiver permissão) e aplica todas as migrations.

---

## Cobertura resumida

| Endpoint | Método | Testes |
|---|---|---|
| `/api/usuarios/criar` | POST | 4 |
| `/api/usuarios/obter/{email}` | GET | 2 |
| **Total** | | **6** |