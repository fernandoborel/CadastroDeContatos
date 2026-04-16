using Bogus;
using Contatos.Domain.Dtos.Response;
using System.Net;
using System.Net.Http.Json;

namespace Contatos.Tests.Integration;

/// <summary>
/// Testes de integração para <see cref="Contatos.Api.Controllers.UsuariosController"/>.
/// Utiliza <see cref="WebApiFactory"/> para subir a aplicação em memória com banco de dados real (ambiente Testing).
/// O banco é limpo antes de cada teste via <see cref="IAsyncLifetime.InitializeAsync"/> para garantir isolamento.
/// </summary>
public class UsuariosControllerIntegrationTests : IClassFixture<WebApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly WebApiFactory _factory;
    private readonly Faker _faker;

    /// <summary>
    /// Injeta a factory compartilhada e cria o cliente HTTP para os testes.
    /// </summary>
    public UsuariosControllerIntegrationTests(WebApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _faker = new Faker("pt_BR");
    }

    /// <summary>Limpa o banco de dados antes de cada teste.</summary>
    public async Task InitializeAsync() => await _factory.LimparBancoDeDadosAsync();

    /// <summary>Sem ações necessárias após cada teste.</summary>
    public Task DisposeAsync() => Task.CompletedTask;

    #region POST /api/usuarios/criar

    /// <summary>
    /// POST /api/usuarios/criar com dados válidos deve retornar 201 Created com o corpo do usuário criado.
    /// </summary>
    [Fact]
    public async Task Criar_QuandoDadosValidos_DeveRetornar201ComCorpoCorreto()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(body);
        Assert.NotEqual(Guid.Empty, body.Id);
        Assert.Equal(corpo.Nome, body.Nome);
        Assert.Equal(corpo.Email.ToLower(), body.Email);
        Assert.True(body.Ativo);
    }

    /// <summary>
    /// POST /api/usuarios/criar com nome inválido (menos de 4 caracteres) deve retornar 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Criar_QuandoNomeInvalido_DeveRetornar400()
    {
        var corpo = new { Nome = "abc", Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// POST /api/usuarios/criar com email sem formato válido deve retornar 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Criar_QuandoEmailInvalido_DeveRetornar400()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = "email_invalido", Senha = _faker.Internet.Password(8) };

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// POST /api/usuarios/criar com email já existente deve retornar 422 Unprocessable Entity.
    /// </summary>
    [Fact]
    public async Task Criar_QuandoEmailJaCadastrado_DeveRetornar422()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        var response = await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion

    #region GET /api/usuarios/obter?email=

    /// <summary>
    /// GET /api/usuarios/obter?email= com email existente deve retornar 200 OK com os dados do usuário.
    /// </summary>
    [Fact]
    public async Task Obter_QuandoUsuarioExiste_DeveRetornar200ComCorpoCorreto()
    {
        var corpo = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        await _client.PostAsJsonAsync("/api/usuarios/criar", corpo);

        var response = await _client.GetAsync($"/api/usuarios/obter?email={corpo.Email}");
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(corpo.Nome, body.Nome);
        Assert.Equal(corpo.Email.ToLower(), body.Email);
    }

    /// <summary>
    /// GET /api/usuarios/obter?email= com email não cadastrado deve retornar 422 Unprocessable Entity.
    /// </summary>
    [Fact]
    public async Task Obter_QuandoUsuarioNaoExiste_DeveRetornar422()
    {
        var response = await _client.GetAsync("/api/usuarios/obter?email=naoexiste@email.com");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion

    #region PUT /api/usuarios/atualizar/{id}

    /// <summary>
    /// PUT /api/usuarios/atualizar/{id} com dados válidos deve retornar 200 OK com o nome atualizado.
    /// </summary>
    [Fact]
    public async Task Atualizar_QuandoDadosValidos_DeveRetornar200ComNomeAtualizado()
    {
        var criar = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        var criado = await (await _client.PostAsJsonAsync("/api/usuarios/criar", criar))
            .Content.ReadFromJsonAsync<UsuarioResponse>();

        var corpo = new { Nome = _faker.Name.FullName() };
        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar/{criado!.Id}", corpo);
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(corpo.Nome, body!.Nome);
    }

    /// <summary>
    /// PUT /api/usuarios/atualizar/{id} com nome inválido deve retornar 400 Bad Request.
    /// </summary>
    [Fact]
    public async Task Atualizar_QuandoNomeInvalido_DeveRetornar400()
    {
        var criar = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        var criado = await (await _client.PostAsJsonAsync("/api/usuarios/criar", criar))
            .Content.ReadFromJsonAsync<UsuarioResponse>();

        var corpo = new { Nome = "ab" };
        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar/{criado!.Id}", corpo);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// PUT /api/usuarios/atualizar/{id} com id inexistente deve retornar 422 Unprocessable Entity.
    /// </summary>
    [Fact]
    public async Task Atualizar_QuandoIdNaoExiste_DeveRetornar422()
    {
        var corpo = new { Nome = _faker.Name.FullName() };
        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar/{Guid.NewGuid()}", corpo);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion

    #region PUT /api/usuarios/atualizar-status/{id}

    /// <summary>
    /// PUT /api/usuarios/atualizar-status/{id} com <c>Ativo = false</c> deve retornar 200 OK com o usuário inativado.
    /// </summary>
    [Fact]
    public async Task AtualizarStatus_QuandoInativar_DeveRetornar200ComAtivoFalso()
    {
        var criar = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        var criado = await (await _client.PostAsJsonAsync("/api/usuarios/criar", criar))
            .Content.ReadFromJsonAsync<UsuarioResponse>();

        var corpo = new { Ativo = false };
        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar-status/{criado!.Id}", corpo);
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(body!.Ativo);
    }

    /// <summary>
    /// PUT /api/usuarios/atualizar-status/{id} com <c>Ativo = true</c> após inativação deve retornar 200 OK com o usuário reativado.
    /// </summary>
    [Fact]
    public async Task AtualizarStatus_QuandoReativar_DeveRetornar200ComAtivoVerdadeiro()
    {
        var criar = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        var criado = await (await _client.PostAsJsonAsync("/api/usuarios/criar", criar))
            .Content.ReadFromJsonAsync<UsuarioResponse>();

        await _client.PutAsJsonAsync($"/api/usuarios/atualizar-status/{criado!.Id}", new { Ativo = false });

        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar-status/{criado.Id}", new { Ativo = true });
        var body = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(body!.Ativo);
    }

    /// <summary>
    /// PUT /api/usuarios/atualizar-status/{id} com id inexistente deve retornar 422 Unprocessable Entity.
    /// </summary>
    [Fact]
    public async Task AtualizarStatus_QuandoIdNaoExiste_DeveRetornar422()
    {
        var corpo = new { Ativo = false };
        var response = await _client.PutAsJsonAsync($"/api/usuarios/atualizar-status/{Guid.NewGuid()}", corpo);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    #endregion

    #region GET /api/usuarios/listar

    /// <summary>
    /// GET /api/usuarios/listar com banco populado deve retornar 200 OK com
    /// <see cref="PagedResponse{UsuarioResponse}"/> preenchido corretamente.
    /// </summary>
    [Fact]
    public async Task Listar_QuandoExistemUsuarios_DeveRetornar200ComPagedResponseCorreto()
    {
        var usuario1 = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        var usuario2 = new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) };
        await _client.PostAsJsonAsync("/api/usuarios/criar", usuario1);
        await _client.PostAsJsonAsync("/api/usuarios/criar", usuario2);

        var response = await _client.GetAsync("/api/usuarios/listar?pagina=1&tamanhoPagina=10");
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Equal(2, body.Total);
        Assert.Equal(2, body.Data.Count());
        Assert.Equal(1, body.Pagina);
        Assert.Equal(10, body.TamanhoPagina);
        Assert.Equal(1, body.TotalPaginas);
    }

    /// <summary>
    /// GET /api/usuarios/listar com banco vazio deve retornar 200 OK com lista vazia e <c>Total = 0</c>.
    /// </summary>
    [Fact]
    public async Task Listar_QuandoBancoVazio_DeveRetornar200ComListaVazia()
    {
        var response = await _client.GetAsync("/api/usuarios/listar?pagina=1&tamanhoPagina=10");
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(body);
        Assert.Empty(body.Data);
        Assert.Equal(0, body.Total);
        Assert.Equal(0, body.TotalPaginas);
    }

    /// <summary>
    /// GET /api/usuarios/listar com paginação deve retornar somente os registros da página solicitada.
    /// </summary>
    [Fact]
    public async Task Listar_QuandoPaginando_DeveRetornarApenasRegistrosDaPagina()
    {
        for (var i = 0; i < 3; i++)
            await _client.PostAsJsonAsync("/api/usuarios/criar", new { Nome = _faker.Name.FullName(), Email = _faker.Internet.Email(), Senha = _faker.Internet.Password(8) });

        var response = await _client.GetAsync("/api/usuarios/listar?pagina=1&tamanhoPagina=2");
        var body = await response.Content.ReadFromJsonAsync<PagedResponse<UsuarioResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(3, body!.Total);
        Assert.Equal(2, body.Data.Count());
        Assert.Equal(2, body.TotalPaginas);
    }

    /// <summary>
    /// GET /api/usuarios/listar com <c>pagina = 0</c> deve retornar 400 Bad Request.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Listar_QuandoPaginaInvalida_DeveRetornar400(int pagina)
    {
        var response = await _client.GetAsync($"/api/usuarios/listar?pagina={pagina}&tamanhoPagina=10");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    /// <summary>
    /// GET /api/usuarios/listar com <c>tamanhoPagina = 0</c> deve retornar 400 Bad Request.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Listar_QuandoTamanhoPaginaInvalido_DeveRetornar400(int tamanhoPagina)
    {
        var response = await _client.GetAsync($"/api/usuarios/listar?pagina=1&tamanhoPagina={tamanhoPagina}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    #endregion
}
