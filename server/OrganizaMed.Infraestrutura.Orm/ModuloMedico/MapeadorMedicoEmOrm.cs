using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Infraestrutura.Orm.ModuloMedico;

public class MapeadorMedicoEmOrm : IEntityTypeConfiguration<Medico>
{
    public void Configure(EntityTypeBuilder<Medico> modelBuilder)
    {
        modelBuilder.ToTable("TBMedico");

        modelBuilder.Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Property(b => b.Nome)
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        modelBuilder.Property(b => b.Crm)
            .HasColumnType("char(8)")
            .IsRequired();

        modelBuilder
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}
