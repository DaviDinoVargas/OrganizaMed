using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarPorId;

public record SelecionarPacientePorIdRequest(Guid Id) : IRequest<Result<SelecionarPacientePorIdResponse>>;