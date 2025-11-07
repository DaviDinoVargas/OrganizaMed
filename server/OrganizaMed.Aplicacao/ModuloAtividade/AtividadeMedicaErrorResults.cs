using FluentResults;

namespace OrganizaMed.Aplicacao.ModuloAtividade;

public abstract class AtividadeMedicaErrorResults
{
    public static Error MedicosNaoEncontradosError()
    {
        return new Error("Médico(s) requisitado(s) não encontrado(s)")
            .CausedBy("Não foi possível obter o(s) médico(s) informado(s) na requisição")
            .WithMetadata("ErrorType", "BadRequest");
    }
    
    public static Error PacienteNaoEncontradoError()
    {
        return new Error("Paciente requisitado não encontrado(s)")
            .CausedBy("Não foi possível obter o paciente informado na requisição")
            .WithMetadata("ErrorType", "BadRequest");
    }


    public static Error ConflitosDePeriodoError(Guid atividadeId)
    {
        return new Error("Conflito de períodos ao tentar registrar atividade")
            .CausedBy($"Já existe uma atividade em conflito com o período informado. ID da atividade: {atividadeId.ToString()}")
            .WithMetadata("ErrorType", "BadRequest"); ;
    }
}