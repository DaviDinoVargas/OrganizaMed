using FluentResults;
using MediatR;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.Inserir;

public record InserirPacienteRequest(string Nome, string Cpf, string Email, string Telefone) : IRequest<Result<InserirPacienteResponse>>;