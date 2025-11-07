using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Infraestrutura.Orm.ModuloPaciente;

public class MapeadorPacienteEmOrm : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> modelBuilder)
    {
        modelBuilder.ToTable("TBPaciente");

        modelBuilder.Property(x => x.Id)
            .ValueGeneratedNever();

        modelBuilder.Property(p => p.Nome)
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        modelBuilder.Property(p => p.Cpf)
            .HasColumnType("char(14)") // Considerando o formato 000.000.000-00
            .IsRequired();

        // Email
        modelBuilder.Property(p => p.Email)
            .HasColumnType("nvarchar(100)")
            .IsRequired();

        // Telefone
        modelBuilder.Property(p => p.Telefone)
            .HasColumnType("varchar(15)") // Considerando o formato (00) 00000-0000 (variável)
            .IsRequired();

        modelBuilder
            .HasMany(p => p.Atividades)
            .WithOne(a => a.Paciente)
            .HasForeignKey(a => a.PacienteId)
            .OnDelete(DeleteBehavior.NoAction);
        
        modelBuilder
            .HasOne(a => a.Usuario)
            .WithMany()
            .HasForeignKey(a => a.UsuarioId)
            .IsRequired()
            .OnDelete(DeleteBehavior.NoAction);
    }
}