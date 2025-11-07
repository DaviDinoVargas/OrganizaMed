using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.Excluir;

public class ExcluirMedicoRequestHandler(
    IRepositorioMedico repositorioMedico,
    IContextoPersistencia contexto
) : IRequestHandler<ExcluirMedicoRequest, Result<ExcluirMedicoResponse>>
{
    public async Task<Result<ExcluirMedicoResponse>> Handle(ExcluirMedicoRequest request, CancellationToken cancellationToken)
    {
        var medicoSelecionado = await repositorioMedico.SelecionarPorIdAsync(request.Id);

        if (medicoSelecionado is null)
            return Result.Fail(ErrorResults.NotFoundError(request.Id));

        try
        {
            await repositorioMedico.ExcluirAsync(medicoSelecionado);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();

            return Result.Fail(ErrorResults.InternalServerError(ex));
        }

        return Result.Ok(new ExcluirMedicoResponse());
    }
}