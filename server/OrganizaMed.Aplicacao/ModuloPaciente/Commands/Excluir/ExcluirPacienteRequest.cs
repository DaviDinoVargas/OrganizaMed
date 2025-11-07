using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.Excluir;

public record ExcluirPacienteRequest(Guid Id) : IRequest<Result<ExcluirPacienteResponse>>;