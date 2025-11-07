using FluentResults;

namespace OrganizaMed.Aplicacao.ModuloMedico;

public abstract class MedicoErrorResults
{
    public static Error NomeDuplicadoError(string nome)
    {
        return new Error("Nome duplicado")
            .CausedBy($"Um médico com o nome '{nome}' já foi cadastrado")
            .WithMetadata("ErrorType", "BadRequest");
    }

    public static Error CrmDuplicadoError(string crm)
    {
        return new Error("CRM duplicado")
            .CausedBy($"Um médico com o CRM '{crm}' já foi cadastrado")
            .WithMetadata("ErrorType", "BadRequest");
    }
}