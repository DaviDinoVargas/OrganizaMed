using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.Excluir;

public class ExcluirAtividadeMedicaRequestHandler(
    IRepositorioAtividadeMedica repositorioAtividadeMedica,
    IContextoPersistencia contexto
) : IRequestHandler<ExcluirAtividadeMedicaRequest, Result<ExcluirAtividadeMedicaResponse>>
{
    public async Task<Result<ExcluirAtividadeMedicaResponse>> Handle(
        ExcluirAtividadeMedicaRequest request, CancellationToken cancellationToken)
    {
        var atividadeSelecionada = await repositorioAtividadeMedica.SelecionarPorIdAsync(request.Id);

        if (atividadeSelecionada is null)
            return Result.Fail(ErrorResults.NotFoundError(request.Id));

        try
        {
            await repositorioAtividadeMedica.ExcluirAsync(atividadeSelecionada);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();
            
            return Result.Fail(ErrorResults.InternalServerError(ex));
        }
        
        return Result.Ok(new ExcluirAtividadeMedicaResponse());
    }
}