# Testes Unitários

## Visão geral

Os testes unitários cobrem as duas camadas com mais lógica de negócio: o **Value Object `Email`** e o **`UsuarioService`**. A ideia foi manter cada teste pequeno e focado em uma única coisa, sem depender de banco de dados ou qualquer infraestrutura externa.

**Bibliotecas utilizadas:**
- `xUnit` — framework de testes
- `Moq` — criação dos mocks de repositório e unit of work
- `Bogus` — geração de dados falsos realistas em pt-BR

---

## Email (Value Object)

**Arquivo:** `Contatos.Tests/ValueObjects/EmailTests.cs`

O `Email` é um Value Object que já valida o endereço no próprio construtor. Toda a lógica fica encapsulada ali, então faz sentido testar ele de forma isolada, sem precisar subir nada além do próprio objeto.

### Testes

| Método | Tipo | Cenário |
|---|---|---|
| `Criar_QuandoEnderecoValido_DeveArmazenarEmMinusculas` | Theory | Endereços válidos devem ser salvos sempre em minúsculo |
| `Criar_QuandoEnderecoVazioOuEspacos_DeveLancarArgumentException` | Theory | String vazia ou só com espaços lança `ArgumentException` |
| `Criar_QuandoEnderecoNulo_DeveLancarArgumentException` | Fact | `null` também lança `ArgumentException` |
| `Criar_QuandoFormatoInvalido_DeveLancarArgumentException` | Theory | Endereços sem `@`, sem domínio completo, etc. |
| `ToString_DeveRetornarEndereco` | Fact | `ToString()` retorna o endereço normalizado |

### Casos cobertos nos Theories

**Endereços válidos** (`Criar_QuandoEnderecoValido_DeveArmazenarEmMinusculas`):
```
"usuario@email.com"
"usuario.nome@dominio.com.br"
"USUARIO@EMAIL.COM"              ← precisa sair em minúsculo
```

**Formatos inválidos** (`Criar_QuandoFormatoInvalido_DeveLancarArgumentException`):
```
"semArroba"       ← falta o @
"sem@dominio"     ← domínio sem extensão
"@semlocal.com"   ← falta a parte local
```

### O que ficou de fora

Propositalmente não testei a regex em si — isso seria testar a implementação, não o comportamento. O que importa é que endereços inválidos são rejeitados e válidos são aceitos.

---

## UsuarioService

**Arquivo:** `Contatos.Tests/Services/UsuarioServiceTests.cs`

O service tem as duas operações principais da aplicação: criar e obter usuário. Como ele depende do `IUnitOfWork` (que por sua vez acessa o banco), tudo foi mockado com Moq para não precisar de nenhuma infraestrutura real aqui.

### Setup do construtor

```csharp
_uowMock = new Mock<IUnitOfWork>();
_repoMock = new Mock<IUsuarioRepository>();
_uowMock.Setup(u => u.UsuarioRepository).Returns(_repoMock.Object);
_service = new UsuarioService(_uowMock.Object);
_faker = new Faker("pt_BR");
```

O `IUnitOfWork` agrega o repositório, então o mock do `IUnitOfWork` precisa ser configurado para devolver o mock do repositório. Usei `pt_BR` no Faker para os nomes ficarem mais próximos do real.

Existe também um método auxiliar `GerarRequestValido()` que evita repetição nos testes que precisam de um request com dados válidos:

```csharp
private UsuarioRequest GerarRequestValido() => new(
    _faker.Name.FullName(),
    _faker.Internet.Email()
);
```

---

### CriarAsync

| Método | Tipo | O que verifica |
|---|---|---|
| `CriarAsync_QuandoDadosValidos_DeveRetornarUsuarioResponse` | Fact | Retorno correto: nome, email em minúsculo, `Ativo = true` |
| `CriarAsync_QuandoDadosValidos_DeveInvocarAddESaveChanges` | Fact | `AddAsync` e `SaveChangesAsync` foram chamados exatamente uma vez |
| `CriarAsync_QuandoNomeInvalido_DeveLancarArgumentException` | Theory | Nomes inválidos: `""`, `"   "`, `"abc"` (menos de 6 chars) |
| `CriarAsync_QuandoEmailInvalido_DeveLancarArgumentException` | Fact | Email sem formato válido dispara `ArgumentException` do Value Object |
| `CriarAsync_QuandoEmailJaCadastrado_DeveLancarApplicationException` | Fact | `CountAsync` retorna 1 → deve lançar `ApplicationException` |

Separei em dois testes o caminho feliz do `CriarAsync`: um verifica o retorno e o outro verifica os efeitos colaterais (chamadas ao repositório). Podia ter juntado tudo num só, mas achei mais claro assim — se um quebrar, fica óbvio o que falhou.

O `CriarAsync_QuandoNomeInvalido` cobre três variações porque a regra tem dois gatilhos diferentes: `string.IsNullOrEmpty` e `Trim().Length < 6`. Os três casos exercitam ambos os lados.

---

### ObterAsync

| Método | Tipo | O que verifica |
|---|---|---|
| `ObterAsync_QuandoUsuarioExiste_DeveRetornarUsuarioResponse` | Fact | Dados do usuário retornados corretamente |
| `ObterAsync_QuandoUsuarioNaoExiste_DeveLancarApplicationException` | Fact | Repositório retornando `null` deve lançar `ApplicationException` |

No teste de sucesso, o `Usuario` é construído diretamente com o Value Object `Email`, porque o mock do repositório precisa devolver um objeto já montado. O email é comparado em minúsculo porque o Value Object normaliza na criação.

---

## Cobertura resumida

| Camada | Classe | Testes | Casos totais |
|---|---|---|---|
| Domain | `Email` | 5 | 11 (incluindo Theory cases) |
| Domain | `UsuarioService` | 7 | 9 (incluindo Theory cases) |
| **Total** | | **12** | **20** |