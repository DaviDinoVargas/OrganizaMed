using OrganizaMed.Dominio.ModuloAutenticacao;

namespace OrganizaMed.Dominio.Compartilhado;

public abstract class EntidadeBase
{
    public Guid Id { get; set; }

    protected EntidadeBase()
    {
        Id = Guid.NewGuid();
    }

    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
}