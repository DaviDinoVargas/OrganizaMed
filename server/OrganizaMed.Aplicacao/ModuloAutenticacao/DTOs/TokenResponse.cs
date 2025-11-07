using OrganizaMed.Dominio.ModuloAutenticacao;

namespace OrganizaMed.Aplicacao.ModuloAutenticacao.DTOs;

public class TokenResponse : IAccessToken
{
    public required string Chave { get; set; }
    public required DateTime DataExpiracao { get; set; }
    public required UsuarioAutenticadoDto Usuario { get; set; }
}