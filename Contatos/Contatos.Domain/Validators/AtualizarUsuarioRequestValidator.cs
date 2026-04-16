using Contatos.Domain.Dtos.Request;
using FluentValidation;

namespace Contatos.Domain.Validators;

public class AtualizarUsuarioRequestValidator : AbstractValidator<AtualizarUsuarioRequest>
{
    public AtualizarUsuarioRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id do usuário é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(4).WithMessage("Nome deve ter pelo menos 4 caracteres.");
    }
}
