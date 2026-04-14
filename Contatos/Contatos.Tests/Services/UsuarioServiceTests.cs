using Bogus;
using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Entities;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Services;
using Contatos.Domain.ValueObjects;
using Moq;
using System.Linq.Expressions;

namespace Contatos.Tests.Services;

public class UsuarioServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly UsuarioService _service;
    private readonly Faker _faker;

    public UsuarioServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IUsuarioRepository>();
        _uowMock.Setup(u => u.UsuarioRepository).Returns(_repoMock.Object);
        _service = new UsuarioService(_uowMock.Object);
        _faker = new Faker("pt_BR");
    }

    private UsuarioRequest GerarRequestValido() => new(
        _faker.Name.FullName(),
        _faker.Internet.Email()
    );

    #region CriarAsync

    [Fact]
    public async Task CriarAsync_QuandoDadosValidos_DeveRetornarUsuarioResponse()
    {
        var request = GerarRequestValido();
        _repoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(0);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.CriarAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Nome, result.Nome);
        Assert.Equal(request.Email.ToLower(), result.Email);
        Assert.True(result.Ativo);
    }

    [Fact]
    public async Task CriarAsync_QuandoDadosValidos_DeveInvocarAddESaveChanges()
    {
        var request = GerarRequestValido();
        _repoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(0);
        _repoMock.Setup(r => r.AddAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.CriarAsync(request);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Usuario>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public async Task CriarAsync_QuandoNomeInvalido_DeveLancarArgumentException(string nome)
    {
        var request = new UsuarioRequest(nome, _faker.Internet.Email());
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(request));
    }

    [Fact]
    public async Task CriarAsync_QuandoEmailInvalido_DeveLancarArgumentException()
    {
        var request = new UsuarioRequest("Nome Valido", "email_invalido");
        await Assert.ThrowsAsync<ArgumentException>(() => _service.CriarAsync(request));
    }

    [Fact]
    public async Task CriarAsync_QuandoEmailJaCadastrado_DeveLancarApplicationException()
    {
        var request = GerarRequestValido();
        _repoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(1);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.CriarAsync(request));
    }

    #endregion

    #region ObterAsync

    [Fact]
    public async Task ObterAsync_QuandoUsuarioExiste_DeveRetornarUsuarioResponse()
    {
        var email = _faker.Internet.Email();
        var usuario = new Usuario { Nome = _faker.Name.FullName(), Email = new Email(email) };
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(usuario);

        var result = await _service.ObterAsync(email);

        Assert.NotNull(result);
        Assert.Equal(usuario.Nome, result.Nome);
        Assert.Equal(email.ToLower(), result.Email);
    }

    [Fact]
    public async Task ObterAsync_QuandoUsuarioNaoExiste_DeveLancarApplicationException()
    {
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.ObterAsync("naoexiste@email.com"));
    }

    #endregion
}