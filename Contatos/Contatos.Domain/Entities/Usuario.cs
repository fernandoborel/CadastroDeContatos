using Contatos.Domain.Commons;
using Contatos.Domain.ValueObjects;

namespace Contatos.Domain.Entities;

public class Usuario : BaseEntity
{
    public string Nome { get; set; }
    public Email? Email { get; set; }
    public string Senha { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}