using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.Excluir;

public class ExcluirPacienteRequestHandler(
    IRepositorioPaciente repositorioPaciente,
    IContextoPersistencia contexto
) : IRequestHandler<ExcluirPacienteRequest, Result<ExcluirPacienteResponse>>
{
    public async Task<Result<ExcluirPacienteResponse>> Handle(ExcluirPacienteRequest request, CancellationToken cancellationToken)
    {
        var medicoSelecionado = await repositorioPaciente.SelecionarPorIdAsync(request.Id);

        if (medicoSelecionado is null)
            return Result.Fail(ErrorResults.NotFoundError(request.Id));

        try
        {
            await repositorioPaciente.ExcluirAsync(medicoSelecionado);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();

            return Result.Fail(ErrorResults.InternalServerError(ex));
        }

        return Result.Ok(new ExcluirPacienteResponse());
    }
}