namespace OrganizaMed.Dominio.Compartilhado;

public interface IContextoPersistencia
{
    Task<int> GravarAsync();
    Task RollbackAsync();
}