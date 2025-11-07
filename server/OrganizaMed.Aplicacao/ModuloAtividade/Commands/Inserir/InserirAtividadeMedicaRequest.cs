using FluentResults;
using MediatR;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.Inserir;

public record InserirAtividadeMedicaRequest(
    Guid PacienteId,
    DateTime Inicio,
    DateTime Termino,
    TipoAtividadeMedica TipoAtividade,
    IEnumerable<Guid> Medicos
) : IRequest<Result<InserirAtividadeMedicaResponse>>;