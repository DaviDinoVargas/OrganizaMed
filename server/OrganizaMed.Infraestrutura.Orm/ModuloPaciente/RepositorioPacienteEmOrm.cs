using Microsoft.EntityFrameworkCore;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloPaciente;
using OrganizaMed.Infraestrutura.Orm.Compartilhado;

namespace OrganizaMed.Infraestrutura.Orm.ModuloPaciente;

public class RepositorioPacienteEmOrm(IContextoPersistencia context)
    : RepositorioBase<Paciente>(context), IRepositorioPaciente
{
    public override Task<Paciente?> SelecionarPorIdAsync(Guid id)
    {
        return registros.Include(p => p.Atividades).ThenInclude(a => a.Medicos).FirstOrDefaultAsync(p => p.Id == id);
    }
}