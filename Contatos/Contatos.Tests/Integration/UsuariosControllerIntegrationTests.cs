using Bogus;
using Contatos.Domain.Dtos.Response;
using System.Net;
using System.Net.Http.Json;

namespace Contatos.Tests.Integration;

public class UsuariosControllerIntegrationTests : IClassFixture<WebApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApiFactory _factory;
    private readonly Faker _faker;

    public UsuariosControllerIntegrationTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _faker = new Faker("pt_BR");
    }

    public async Task InitializeAsync() => await _factory.LimparBancoDeDadosAsync();

    public Task DisposeAsync() => Task.CompletedTask;

    #region POST /api/usuarios/criar

    [Fact]
    public async Task Criar_QuandoDadosValidos_DeveRetornar201ComCorpoCorreto()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email() };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(corpo.Nome, body.Nome);
        Assert.Equal(corpo.Email.ToLower(), body.Email);
        Assert.True(body.Ativo);
    }

    [Fact]
    public async Task Criar_QuandoNomeInvalido_DeveRetornar400()
    {
        var corpo = new { Nome = "abc", Email = _faker.Internet.Email() };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_QuandoEmailInvalido_DeveRetornar400()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = "email_invalido" };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Criar_QuandoEmailJaCadastrado_DeveRetornar422()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email() };
        await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion

    #region GET /api/usuarios/obter/{email}

    [Fact]
    public async Task Obter_QuandoUsuarioExiste_DeveRetornar200ComCorpoCorreto()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email() };
        await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        var response = await _client.GetAsync($"/api/usuarios/obter/{corpo.Email}");
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(corpo.Nome, body.Nome);
        Assert.Equal(corpo.Email.ToLower(), body.Email);
    }

    [Fact]
    public async Task Obter_QuandoUsuarioNaoExiste_DeveRetornar422()
    {
        var response = await _client.GetAsync("/api/usuarios/obter/naoexiste@email.com");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion
}
