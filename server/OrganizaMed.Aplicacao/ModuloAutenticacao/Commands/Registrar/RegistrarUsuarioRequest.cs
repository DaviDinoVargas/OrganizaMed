using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.ModuloAutenticacao.DTOs;

namespace OrganizaMed.Aplicacao.ModuloAutenticacao.Commands.Registrar;

public record RegistrarUsuarioRequest(string UserName, string Email, string Password) 
    : IRequest<Result<TokenResponse>>;