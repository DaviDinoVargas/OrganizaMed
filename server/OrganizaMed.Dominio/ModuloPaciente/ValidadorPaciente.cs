using FluentValidation;

namespace OrganizaMed.Dominio.ModuloPaciente;

public class ValidadorPaciente : AbstractValidator<Paciente>
{
    public ValidadorPaciente()
    {
        RuleFor(p => p.Nome)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório")
            .MinimumLength(3).WithMessage("O campo {PropertyName} deve conter no mínimo {MinLength} caracteres");

        RuleFor(p => p.Cpf)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório")
            .Matches(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$")
            .WithMessage("O campo {PropertyName} deve seguir o formato 000.000.000-00");

        RuleFor(p => p.Email)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório")
            .EmailAddress().WithMessage("O campo {PropertyName} deve conter um endereço de e-mail válido");

        RuleFor(p => p.Telefone)
            .NotEmpty().WithMessage("O campo {PropertyName} é obrigatório")
            .Matches(@"^\(\d{2}\)\s\d{4,5}-\d{4}$")
            .WithMessage("O campo {PropertyName} deve seguir o formato (00) 00000-0000");
    }
}