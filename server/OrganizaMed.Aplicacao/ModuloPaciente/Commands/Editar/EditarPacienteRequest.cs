using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.Editar;

public record EditarPacientePartialRequest(string Nome, string Cpf, string Email, string Telefone);

public record EditarPacienteRequest(Guid Id, string Nome, string Cpf, string Email, string Telefone) : IRequest<Result<EditarPacienteResponse>>;