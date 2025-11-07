using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.SelecionarPorId;

public record SelecionarAtividadeMedicaPorIdRequest(Guid Id) 
    : IRequest<Result<SelecionarAtividadeMedicaPorIdResponse>>;

