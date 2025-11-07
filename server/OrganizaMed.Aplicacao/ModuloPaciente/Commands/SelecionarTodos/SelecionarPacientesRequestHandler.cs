using FluentResults;
using MediatR;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarTodos;

public class SelecionarPacientesRequestHandler(
    IRepositorioPaciente repositorioPaciente
) : IRequestHandler<SelecionarPacientesRequest, Result<SelecionarPacientesResponse>>
{
    public async Task<Result<SelecionarPacientesResponse>> Handle(
        SelecionarPacientesRequest request, CancellationToken cancellationToken)
    {
        var registros = await repositorioPaciente.SelecionarTodosAsync();

        var response = new SelecionarPacientesResponse
        {
            QuantidadeRegistros = registros.Count,
            Registros = registros
                .Select(r => new SelecionarPacientesDto(r.Id, r.Nome, r.Cpf, r.Email, r.Telefone))
        };

        return Result.Ok(response);
    }
}