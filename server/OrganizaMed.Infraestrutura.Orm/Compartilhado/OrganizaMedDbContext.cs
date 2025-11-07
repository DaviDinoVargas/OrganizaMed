using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloMedico;
using OrganizaMed.Dominio.ModuloPaciente;
using OrganizaMed.Infraestrutura.Orm.ModuloAtividade;
using OrganizaMed.Infraestrutura.Orm.ModuloMedico;
using OrganizaMed.Infraestrutura.Orm.ModuloPaciente;

namespace OrganizaMed.Infraestrutura.Orm.Compartilhado;

public class OrganizaMedDbContext(DbContextOptions options, ITenantProvider? tenantProvider = null)
    : IdentityDbContext<Usuario, Cargo, Guid>(options), IContextoPersistencia
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        if (tenantProvider is not null)
        {
            modelBuilder.Entity<Medico>().HasQueryFilter(m => m.UsuarioId == tenantProvider.UsuarioId);
            modelBuilder.Entity<Paciente>().HasQueryFilter(m => m.UsuarioId == tenantProvider.UsuarioId);
            modelBuilder.Entity<AtividadeMedica>().HasQueryFilter(m => m.UsuarioId == tenantProvider.UsuarioId);
        }

        modelBuilder.ApplyConfiguration(new MapeadorMedicoEmOrm());
        modelBuilder.ApplyConfiguration(new MapeadorPacienteEmOrm());
        modelBuilder.ApplyConfiguration(new MapeadorAtividadeMedicaEmOrm());

        base.OnModelCreating(modelBuilder);
    }

    public async Task<int> GravarAsync()
    {
        return await SaveChangesAsync();
    }

    public async Task RollbackAsync()
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.State = EntityState.Detached;
                    break;
                case EntityState.Modified:
                    entry.State = EntityState.Unchanged;
                    break;
                case EntityState.Deleted:
                    entry.State = EntityState.Unchanged;
                    break;
            }
        }

        await Task.CompletedTask;
    }
}

