using Microsoft.EntityFrameworkCore;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloMedico;
using OrganizaMed.Infraestrutura.Orm.Compartilhado;

namespace OrganizaMed.Infraestrutura.Orm.ModuloMedico;

public class RepositorioMedicoEmOrm(IContextoPersistencia context)
    : RepositorioBase<Medico>(context), IRepositorioMedico
{
    public override Task<Medico?> SelecionarPorIdAsync(Guid id)
    {
        return registros.Include(m => m.Atividades).FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<Medico>> SelecionarMuitosPorId(IEnumerable<Guid> medicosRequisitados)
    {
        return await registros.Where(m => medicosRequisitados.Contains(m.Id)).Include(m => m.Atividades).ToListAsync();
    }

    public async Task<List<RegistroDeHorasTrabalhadas>> SelecionarMedicosMaisAtivosPorPeriodo(
        DateTime inicioPeriodo, DateTime terminoPeriodo)
    {
        return await registros.Select(medico => new RegistroDeHorasTrabalhadas
        {
            MedicoId = medico.Id,
            Medico = medico.Nome,
            TotalDeHorasTrabalhadas = medico.Atividades
            .Where(a => a.Inicio >= inicioPeriodo && a.Termino <= terminoPeriodo)
            .Sum(a => EF.Functions.DateDiffHour(a.Inicio, a.Termino))
            .GetValueOrDefault()
        }).OrderByDescending(m => m.TotalDeHorasTrabalhadas)
        .Take(10)
        .ToListAsync();
    }
}
