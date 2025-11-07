using FluentResults;
using FluentValidation;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.Inserir;

public class InserirMedicoRequestHandler(
    IContextoPersistencia contexto,
    IRepositorioMedico repositorioMedico,
    ITenantProvider tenantProvider,
    IValidator<Medico> validador
) : IRequestHandler<InserirMedicoRequest, Result<InserirMedicoResponse>>
{
    public async Task<Result<InserirMedicoResponse>> Handle(
        InserirMedicoRequest request, CancellationToken cancellationToken)
    {
        var medico = new Medico(request.Nome, request.Crm)
        {
            UsuarioId = tenantProvider.UsuarioId.GetValueOrDefault()
        };

        // validações
        var resultadoValidacao = await validador.ValidateAsync(medico);

        if (!resultadoValidacao.IsValid)
        {
            var erros = resultadoValidacao.Errors
               .Select(failure => failure.ErrorMessage)
               .ToList();

            return Result.Fail(ErrorResults.BadRequestError(erros));
        }

        var medicosRegistrados = await repositorioMedico.SelecionarTodosAsync();

        if (NomeDuplicado(medico, medicosRegistrados))
            return Result.Fail(MedicoErrorResults.NomeDuplicadoError(medico.Nome));

        if (CrmDuplicado(medico, medicosRegistrados))
            return Result.Fail(MedicoErrorResults.CrmDuplicadoError(medico.Crm));

        // inserção
        try
        {
            await repositorioMedico.InserirAsync(medico);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();

            return Result.Fail(ErrorResults.InternalServerError(ex));
        }

        return Result.Ok(new InserirMedicoResponse(medico.Id));
    }

    private bool NomeDuplicado(Medico medico, IList<Medico> medicos)
    {
        return medicos
            .Any(registro => string.Equals(
                registro.Nome,
                medico.Nome,
                StringComparison.CurrentCultureIgnoreCase)
            );
    }

    private bool CrmDuplicado(Medico medico, IList<Medico> medicos)
    {
        return medicos
            .Any(registro => string.Equals(
                    registro.Crm,
                    medico.Crm,
                    StringComparison.CurrentCultureIgnoreCase)
            );
    }
}
