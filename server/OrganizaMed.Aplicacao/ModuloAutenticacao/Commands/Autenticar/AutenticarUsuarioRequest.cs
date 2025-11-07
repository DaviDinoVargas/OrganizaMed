using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.ModuloAutenticacao.DTOs;

namespace OrganizaMed.Aplicacao.ModuloAutenticacao.Commands.Autenticar;

public record AutenticarUsuarioRequest(string UserName, string Password) : IRequest<Result<TokenResponse>>;