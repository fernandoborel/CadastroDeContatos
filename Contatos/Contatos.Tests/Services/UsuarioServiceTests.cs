using Bogus;
using Contatos.Domain.Dtos.Request;
using Contatos.Domain.Dtos.Response;
using Contatos.Domain.Entities;
using Contatos.Domain.Interfaces.Repositories;
using Contatos.Domain.Services;
using Contatos.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using System.Linq.Expressions;
using ValidationException = FluentValidation.ValidationException;
using ValidationResult = FluentValidation.Results.ValidationResult;

namespace Contatos.Tests.Services;

/// <summary>
/// Testes unitários para <see cref="UsuarioService"/>.
/// Todas as dependências externas são substituídas por mocks (Moq),
/// garantindo isolamento total da camada de domínio.
/// </summary>
public class UsuarioServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock;
    private readonly Mock<IUsuarioRepository> _repoMock;
    private readonly Mock<IValidator<UsuarioRequest>> _criarValidatorMock;
    private readonly Mock<IValidator<AtualizarUsuarioRequest>> _atualizarValidatorMock;
    private readonly UsuarioService _service;
    private readonly Faker _faker;

    /// <summary>
    /// Inicializa os mocks e instancia o serviço antes de cada teste.
    /// Os validators são configurados para passar por padrão (cenário feliz).
    /// </summary>
    public UsuarioServiceTests()
    {
        _uowMock = new Mock<IUnitOfWork>();
        _repoMock = new Mock<IUsuarioRepository>();
        _criarValidatorMock = new Mock<IValidator<UsuarioRequest>>();
        _atualizarValidatorMock = new Mock<IValidator<AtualizarUsuarioRequest>>();

        _uowMock.Setup(u => u.UsuarioRepository).Returns(_repoMock.Object);

        _criarValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UsuarioRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _atualizarValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AtualizarUsuarioRequest>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _service = new UsuarioService(_uowMock.Object, _criarValidatorMock.Object, _atualizarValidatorMock.Object);
        _faker = new Faker("pt_BR");
    }

    /// <summary>
    /// Gera um <see cref="UsuarioRequest"/> com dados válidos usando Bogus.
    /// </summary>
    private UsuarioRequest GerarRequestValido() => new(
        _faker.Name.FullName(),
        _faker.Internet.Email(),
        _faker.Internet.Password(8)
    );

    #region CriarAsync

    /// <summary>
    /// Dado dados válidos, deve retornar um <see cref="Contatos.Domain.Dtos.Response.UsuarioResponse"/> preenchido corretamente.
    /// </summary>
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

    /// <summary>
    /// Dado dados válidos, deve invocar <c>AddAsync</c> e <c>SaveChangesAsync</c> exatamente uma vez.
    /// </summary>
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

    /// <summary>
    /// Dado um nome vazio, com apenas espaços ou com menos de 4 caracteres,
    /// deve lançar <see cref="ValidationException"/> via FluentValidation.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public async Task CriarAsync_QuandoNomeInvalido_DeveLancarValidationException(string nome)
    {
        var request = new UsuarioRequest(nome, _faker.Internet.Email(), _faker.Internet.Password(8));
        _criarValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UsuarioRequest>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new[] { new ValidationFailure(nameof(request.Nome), "Nome é obrigatório.") }));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(request));
    }

    /// <summary>
    /// Dado um email sem formato válido, deve lançar <see cref="ValidationException"/> via FluentValidation.
    /// </summary>
    [Fact]
    public async Task CriarAsync_QuandoEmailInvalido_DeveLancarValidationException()
    {
        var request = new UsuarioRequest("Nome Valido", "email_invalido", _faker.Internet.Password(8));
        _criarValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<UsuarioRequest>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new[] { new ValidationFailure(nameof(request.Email), "Email inválido.") }));

        await Assert.ThrowsAsync<ValidationException>(() => _service.CriarAsync(request));
    }

    /// <summary>
    /// Dado um email já existente no banco, deve lançar <see cref="ApplicationException"/>.
    /// </summary>
    [Fact]
    public async Task CriarAsync_QuandoEmailJaCadastrado_DeveLancarApplicationException()
    {
        var request = GerarRequestValido();
        _repoMock.Setup(r => r.CountAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(1);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.CriarAsync(request));
    }

    #endregion

    #region ObterAsync

    /// <summary>
    /// Dado um email existente, deve retornar o <see cref="Contatos.Domain.Dtos.Response.UsuarioResponse"/> correspondente.
    /// </summary>
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

    /// <summary>
    /// Dado um email não cadastrado, deve lançar <see cref="ApplicationException"/>.
    /// </summary>
    [Fact]
    public async Task ObterAsync_QuandoUsuarioNaoExiste_DeveLancarApplicationException()
    {
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.ObterAsync("naoexiste@email.com"));
    }

    #endregion

    #region AtualizarAsync

    /// <summary>
    /// Dado dados válidos, deve retornar o usuário com o nome atualizado.
    /// </summary>
    [Fact]
    public async Task AtualizarAsync_QuandoDadosValidos_DeveRetornarUsuarioComNomeAtualizado()
    {
        var usuario = new Usuario { Nome = "Nome Antigo", Email = new Email(_faker.Internet.Email()) };
        var request = new AtualizarUsuarioRequest(usuario.Id, _faker.Name.FullName());
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AtualizarAsync(request);

        Assert.NotNull(result);
        Assert.Equal(request.Nome, result.Nome);
    }

    /// <summary>
    /// Dado dados válidos, deve invocar <c>UpdateAsync</c> e <c>SaveChangesAsync</c> exatamente uma vez.
    /// </summary>
    [Fact]
    public async Task AtualizarAsync_QuandoDadosValidos_DeveInvocarUpdateESaveChanges()
    {
        var usuario = new Usuario { Nome = "Nome Antigo", Email = new Email(_faker.Internet.Email()) };
        var request = new AtualizarUsuarioRequest(usuario.Id, _faker.Name.FullName());
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.AtualizarAsync(request);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Dado um id inexistente, deve lançar <see cref="ApplicationException"/>.
    /// </summary>
    [Fact]
    public async Task AtualizarAsync_QuandoUsuarioNaoExiste_DeveLancarApplicationException()
    {
        var request = new AtualizarUsuarioRequest(Guid.NewGuid(), _faker.Name.FullName());
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.AtualizarAsync(request));
    }

    /// <summary>
    /// Dado um nome vazio, com apenas espaços ou com menos de 4 caracteres,
    /// deve lançar <see cref="ValidationException"/> sem consultar o banco.
    /// </summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    public async Task AtualizarAsync_QuandoNomeInvalido_DeveLancarValidationException(string nome)
    {
        var request = new AtualizarUsuarioRequest(Guid.NewGuid(), nome);
        _atualizarValidatorMock
            .Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<AtualizarUsuarioRequest>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ValidationException(new[] { new ValidationFailure(nameof(request.Nome), "Nome é obrigatório.") }));

        await Assert.ThrowsAsync<ValidationException>(() => _service.AtualizarAsync(request));

        _repoMock.Verify(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>()), Times.Never);
    }

    #endregion

    #region AtualizarStatusAsync

    /// <summary>
    /// Dado <c>Ativo = true</c> ou <c>Ativo = false</c>, deve persistir o valor corretamente no campo <c>Ativo</c>.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AtualizarStatusAsync_QuandoValido_DeveAtualizarCampoAtivo(bool ativo)
    {
        var usuario = new Usuario { Nome = _faker.Name.FullName(), Email = new Email(_faker.Internet.Email()), Ativo = !ativo };
        var request = new AtualizarStatusUsuarioRequest(usuario.Id, ativo);
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        var result = await _service.AtualizarStatusAsync(request);

        Assert.Equal(ativo, result.Ativo);
    }

    /// <summary>
    /// Dado dados válidos, deve invocar <c>UpdateAsync</c> e <c>SaveChangesAsync</c> exatamente uma vez.
    /// </summary>
    [Fact]
    public async Task AtualizarStatusAsync_QuandoValido_DeveInvocarUpdateESaveChanges()
    {
        var usuario = new Usuario { Nome = _faker.Name.FullName(), Email = new Email(_faker.Internet.Email()) };
        var request = new AtualizarStatusUsuarioRequest(usuario.Id, false);
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync(usuario);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Usuario>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.SaveChangesAsync()).Returns(Task.CompletedTask);

        await _service.AtualizarStatusAsync(request);

        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Usuario>()), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    /// <summary>
    /// Dado um id inexistente, deve lançar <see cref="ApplicationException"/>.
    /// </summary>
    [Fact]
    public async Task AtualizarStatusAsync_QuandoUsuarioNaoExiste_DeveLancarApplicationException()
    {
        var request = new AtualizarStatusUsuarioRequest(Guid.NewGuid(), false);
        _repoMock.Setup(r => r.FirstOrDefaultAsync(It.IsAny<Expression<Func<Usuario, bool>>>())).ReturnsAsync((Usuario?)null);

        await Assert.ThrowsAsync<ApplicationException>(() => _service.AtualizarStatusAsync(request));
    }

    #endregion

    #region ObterTodosAsync

    /// <summary>
    /// Dado uma página e tamanho válidos com registros no banco, deve retornar
    /// um <see cref="PagedResponse{UsuarioResponse}"/> preenchido corretamente.
    /// </summary>
    [Fact]
    public async Task ObterTodosAsync_QuandoExistemUsuarios_DeveRetornarPagedResponseCorreto()
    {
        var usuarios = Enumerable.Range(1, 3)
            .Select(_ => new Usuario { Nome = _faker.Name.FullName(), Email = new Email(_faker.Internet.Email()) })
            .ToList();

        _repoMock
            .Setup(r => r.GetPageAsync(1, 10, null))
            .ReturnsAsync((usuarios, usuarios.Count));

        var result = await _service.ObterTodosAsync(1, 10);

        Assert.NotNull(result);
        Assert.Equal(3, result.Total);
        Assert.Equal(3, result.Data.Count());
        Assert.Equal(1, result.Pagina);
        Assert.Equal(10, result.TamanhoPagina);
        Assert.Equal(1, result.TotalPaginas);
    }

    /// <summary>
    /// Dado uma lista vazia no banco, deve retornar <see cref="PagedResponse{UsuarioResponse}"/>
    /// com <c>Data</c> vazio e <c>Total = 0</c>.
    /// </summary>
    [Fact]
    public async Task ObterTodosAsync_QuandoNaoExistemUsuarios_DeveRetornarPagedResponseVazio()
    {
        _repoMock
            .Setup(r => r.GetPageAsync(1, 10, null))
            .ReturnsAsync((Enumerable.Empty<Usuario>(), 0));

        var result = await _service.ObterTodosAsync(1, 10);

        Assert.NotNull(result);
        Assert.Empty(result.Data);
        Assert.Equal(0, result.Total);
        Assert.Equal(0, result.TotalPaginas);
    }

    /// <summary>
    /// Dado múltiplas páginas, deve calcular <c>TotalPaginas</c> corretamente via teto da divisão.
    /// </summary>
    [Theory]
    [InlineData(10, 3, 4)]
    [InlineData(10, 5, 2)]
    [InlineData(10, 10, 1)]
    [InlineData(11, 10, 2)]
    public async Task ObterTodosAsync_QuandoCalculaTotalPaginas_DeveArredondarParaCima(int total, int tamanhoPagina, int totalPaginasEsperado)
    {
        var usuarios = Enumerable.Range(1, tamanhoPagina)
            .Select(_ => new Usuario { Nome = _faker.Name.FullName(), Email = new Email(_faker.Internet.Email()) })
            .ToList();

        _repoMock
            .Setup(r => r.GetPageAsync(1, tamanhoPagina, null))
            .ReturnsAsync((usuarios, total));

        var result = await _service.ObterTodosAsync(1, tamanhoPagina);

        Assert.Equal(totalPaginasEsperado, result.TotalPaginas);
    }

    /// <summary>
    /// Dado <c>pagina</c> menor que 1, deve lançar <see cref="ArgumentException"/> sem consultar o banco.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ObterTodosAsync_QuandoPaginaMenorQueUm_DeveLancarArgumentException(int pagina)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ObterTodosAsync(pagina, 10));

        _repoMock.Verify(r => r.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), null), Times.Never);
    }

    /// <summary>
    /// Dado <c>tamanhoPagina</c> menor que 1, deve lançar <see cref="ArgumentException"/> sem consultar o banco.
    /// </summary>
    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ObterTodosAsync_QuandoTamanhoPaginaMenorQueUm_DeveLancarArgumentException(int tamanhoPagina)
    {
        await Assert.ThrowsAsync<ArgumentException>(() => _service.ObterTodosAsync(1, tamanhoPagina));

        _repoMock.Verify(r => r.GetPageAsync(It.IsAny<int>(), It.IsAny<int>(), null), Times.Never);
    }

    #endregion
}