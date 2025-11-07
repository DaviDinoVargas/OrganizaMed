using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarPorId;

public record SelecionarMedicoPorIdRequest(Guid Id) : IRequest<Result<SelecionarMedicoPorIdResponse>>;