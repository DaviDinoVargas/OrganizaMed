using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.Excluir;

public record ExcluirAtividadeMedicaRequest(Guid Id) : IRequest<Result<ExcluirAtividadeMedicaResponse>>;