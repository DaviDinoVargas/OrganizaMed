using FluentResults;
using MediatR;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.SelecionarTodos;

public record SelecionarAtividadesMedicasRequest(TipoAtividadeMedica? TipoAtividade)
    : IRequest<Result<SelecionarAtividadesMedicasResponse>>;

