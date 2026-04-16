using Contatos.Domain.Dtos.Request;
using FluentValidation;

namespace Contatos.Domain.Validators;

public class UsuarioRequestValidator : AbstractValidator<UsuarioRequest>
{
    public UsuarioRequestValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(4).WithMessage("Nome deve ter pelo menos 4 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres.");
    }
}
