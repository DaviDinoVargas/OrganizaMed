using FluentResults;
using FluentValidation;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.Editar;

public class EditarMedicoRequestHandler(
    IRepositorioMedico repositorioMedico,
    IContextoPersistencia contexto,
    IValidator<Medico> validador
) : IRequestHandler<EditarMedicoRequest, Result<EditarMedicoResponse>>
{
    public async Task<Result<EditarMedicoResponse>> Handle(EditarMedicoRequest request, CancellationToken cancellationToken)
    {
        var medicoSelecionado = await repositorioMedico.SelecionarPorIdAsync(request.Id);

        if (medicoSelecionado == null)
            return Result.Fail(ErrorResults.NotFoundError(request.Id));
        
        medicoSelecionado.Nome = request.Nome;
        medicoSelecionado.Crm = request.Crm;
        
        var resultadoValidacao = 
            await validador.ValidateAsync(medicoSelecionado, cancellationToken);
        
        if (!resultadoValidacao.IsValid)
        {
            var erros = resultadoValidacao.Errors
                .Select(failure => failure.ErrorMessage)
                .ToList();

            return Result.Fail(ErrorResults.BadRequestError(erros));
        }

        var medicos = await repositorioMedico.SelecionarTodosAsync();

        if (NomeDuplicado(medicoSelecionado, medicos))
            return Result.Fail(MedicoErrorResults.NomeDuplicadoError(medicoSelecionado.Nome));
        
        if (CrmDuplicado(medicoSelecionado, medicos))
            return Result.Fail(MedicoErrorResults.CrmDuplicadoError(medicoSelecionado.Crm));
        
        try
        {
            await repositorioMedico.EditarAsync(medicoSelecionado);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();
            
            return Result.Fail(ErrorResults.InternalServerError(ex));
        }
 
        return Result.Ok(new EditarMedicoResponse(medicoSelecionado.Id));
    }
    
    private bool NomeDuplicado(Medico medico, IList<Medico> medicos)
    {
        return medicos
            .Where(r => r.Id != medico.Id)
            .Any(registro => string.Equals(
                registro.Nome,
                medico.Nome,
                StringComparison.CurrentCultureIgnoreCase)
            );
    }
    
    private bool CrmDuplicado(Medico medico, IList<Medico> medicos)
    {
        return medicos
            .Where(r => r.Id != medico.Id)
            .Any(registro => string.Equals(
                registro.Crm,
                medico.Crm,
                StringComparison.CurrentCultureIgnoreCase)
            );
    }
}