using FluentResults;
using MediatR;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;

public class SelecionarMedicosMaisAtivosRequestHandler(
    IRepositorioMedico repositorioMedico
) : IRequestHandler<SelecionarMedicosMaisAtivosRequest, Result<SelecionarMedicosMaisAtivosResponse>>
{
    public async Task<Result<SelecionarMedicosMaisAtivosResponse>> Handle(SelecionarMedicosMaisAtivosRequest request, CancellationToken cancellationToken)
    {
        var registros = await repositorioMedico.SelecionarMedicosMaisAtivosPorPeriodo(
            request.inicioPeriodo,
            request.terminoPeriodo
        );

        var response = new SelecionarMedicosMaisAtivosResponse
        {
            QuantidadeRegistros = registros.Count,
            Registros = registros.Select(m => new SelecionarRegistroDeHorasTrabalhadasDto(
                m.MedicoId, m.Medico, m.TotalDeHorasTrabalhadas
            ))
        };

        return Result.Ok(response);
    }
}