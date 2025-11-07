using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.Editar;

public record EditarAtividadeMedicaRequest(
    Guid Id,
    DateTime Inicio,
    DateTime Termino,
    IEnumerable<Guid> Medicos
) : IRequest<Result<EditarAtividadeMedicaResponse>>;