using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;

public record SelecionarMedicosRequest : IRequest<Result<SelecionarMedicosResponse>>;