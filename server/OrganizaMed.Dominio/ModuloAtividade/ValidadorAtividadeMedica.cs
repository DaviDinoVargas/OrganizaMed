using FluentValidation;

namespace OrganizaMed.Dominio.ModuloAtividade;

public class ValidadorAtividadeMedica : AbstractValidator<AtividadeMedica>
{
    public ValidadorAtividadeMedica()
    {
        RuleFor(a => a.Termino)
            .Must((atividade, termino) => termino == null || termino > atividade.Inicio)
            .WithMessage("A data de término deve ser posterior à data de início");

        RuleFor(a => a)
            .Must(ValidarMedicosParaConsulta)
            .WithMessage("Consultas só podem ser realizadas por um médico");

        RuleForEach(a => a.Medicos)
            .Must((atividade, medico) => medico.PeriodoDeDescansoEstaValido(atividade))
            .WithMessage((_, medico) => $"O médico '{medico.Nome}' está em período de descanso obrigatório");
    }

    private bool ValidarMedicosParaConsulta(AtividadeMedica atividade)
    {
        if (atividade.TipoAtividade == TipoAtividadeMedica.Consulta)
            return atividade.Medicos.Count == 1;

        return true;
    }
}