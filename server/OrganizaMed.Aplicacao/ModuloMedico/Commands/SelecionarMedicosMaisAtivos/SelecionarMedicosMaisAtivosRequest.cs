using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;

public record SelecionarMedicosMaisAtivosRequest(DateTime inicioPeriodo, DateTime terminoPeriodo) :
    IRequest<Result<SelecionarMedicosMaisAtivosResponse>>;