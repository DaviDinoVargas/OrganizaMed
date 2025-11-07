using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.Inserir;

public record InserirMedicoRequest(string Nome, string Crm)
    : IRequest<Result<InserirMedicoResponse>>;
