using FluentResults;
using MediatR;
using Microsoft.AspNetCore.Identity;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloAutenticacao.DTOs;
using OrganizaMed.Dominio.ModuloAutenticacao;

namespace OrganizaMed.Aplicacao.ModuloAutenticacao.Commands.Registrar;

public class RegistrarUsuarioRequestHandler(
    UserManager<Usuario> userManager,
    ITokenProvider tokenProvider
) : IRequestHandler<RegistrarUsuarioRequest, Result<TokenResponse>>
{
    public async Task<Result<TokenResponse>> Handle(
        RegistrarUsuarioRequest request, CancellationToken cancellationToken)
    {
        var usuario = new Usuario
        {
            UserName = request.UserName,
            Email = request.Email
        };
        
        var usuarioResult = await userManager.CreateAsync(usuario, request.Password);

        if (!usuarioResult.Succeeded)
        {
            var erros = usuarioResult
                .Errors
                .Select(failure => failure.Description)
                .ToList();

            return Result.Fail(ErrorResults.BadRequestError(erros));
        }

        var tokenAcesso = tokenProvider.GerarTokenDeAcesso(usuario) as TokenResponse;

        if (tokenAcesso == null)
            return Result.Fail(ErrorResults.InternalServerError(new Exception("Falha ao gerar token de acesso")));
        
        return Result.Ok(tokenAcesso);
    }
}