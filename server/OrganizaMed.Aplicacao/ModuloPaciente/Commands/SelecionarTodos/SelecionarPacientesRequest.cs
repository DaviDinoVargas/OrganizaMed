using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarTodos;

public record SelecionarPacientesRequest() : IRequest<Result<SelecionarPacientesResponse>>;