using Contatos.Domain.ValueObjects;

namespace Contatos.Tests.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("usuario@email.com")]
    [InlineData("usuario.nome@dominio.com.br")]
    [InlineData("USUARIO@EMAIL.COM")]
    public void Criar_QuandoEnderecoValido_DeveArmazenarEmMinusculas(string endereco)
    {
        var email = new Email(endereco);
        Assert.Equal(endereco.ToLower(), email.Endereco);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Criar_QuandoEnderecoVazioOuEspacos_DeveLancarArgumentException(string endereco)
    {
        Assert.Throws<ArgumentException>(() => new Email(endereco));
    }

    [Fact]
    public void Criar_QuandoEnderecoNulo_DeveLancarArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Email(null!));
    }

    [Theory]
    [InlineData("semArroba")]
    [InlineData("sem@dominio")]
    [InlineData("@semlocal.com")]
    public void Criar_QuandoFormatoInvalido_DeveLancarArgumentException(string endereco)
    {
        Assert.Throws<ArgumentException>(() => new Email(endereco));
    }

    [Fact]
    public void ToString_DeveRetornarEndereco()
    {
        var email = new Email("usuario@email.com");
        Assert.Equal("usuario@email.com", email.ToString());
    }
}