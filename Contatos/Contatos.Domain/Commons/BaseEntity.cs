namespace Contatos.Domain.Commons;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}