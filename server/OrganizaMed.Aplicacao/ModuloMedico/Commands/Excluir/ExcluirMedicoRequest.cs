using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.Excluir;

public record ExcluirMedicoRequest(Guid Id) : IRequest<Result<ExcluirMedicoResponse>>;