using System.Security.Claims;
using OrganizaMed.Dominio.ModuloAutenticacao;

namespace OrganizaMed.WebApi.Identity;

public class ApiTenantProvider(IHttpContextAccessor contextAccessor) : ITenantProvider
{
    public Guid? UsuarioId
    {
        get
        {
            var claimId = contextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier);

            if (claimId == null)
                return null;

            return Guid.Parse(claimId.Value);
        }
    }
}