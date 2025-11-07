using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarPorId;

public class SelecionarPacientePorIdRequestHandler(
    IRepositorioPaciente repositorioPaciente
) : IRequestHandler<SelecionarPacientePorIdRequest, Result<SelecionarPacientePorIdResponse>>
{
    public async Task<Result<SelecionarPacientePorIdResponse>> Handle(
        SelecionarPacientePorIdRequest request, CancellationToken cancellationToken)
    {
        var pacienteSelecionado = await repositorioPaciente.SelecionarPorIdAsync(request.Id);

        if (pacienteSelecionado is null)
            return Result.Fail(ErrorResults.NotFoundError(request.Id));

        var resposta = new SelecionarPacientePorIdResponse(
            pacienteSelecionado.Id,
            pacienteSelecionado.Nome,
            pacienteSelecionado.Cpf,
            pacienteSelecionado.Email,
            pacienteSelecionado.Telefone,
            pacienteSelecionado.Atividades.Select(a => new SelecionarAtividadePacienteDto(
                a.Id,
                a.Inicio,
                a.Termino.GetValueOrDefault(),
                a.TipoAtividade,
                a.Medicos.Select(m => new SelecionarMedicosDto(m.Id, m.Nome, m.Crm))
            ))
        );

        return Result.Ok(resposta);
    }
}