using Microsoft.EntityFrameworkCore;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Infraestrutura.Orm.Compartilhado;

namespace OrganizaMed.Infraestrutura.Orm.ModuloAtividade;

public class RepositorioAtividadeMedicaEmOrm(IContextoPersistencia context)
    : RepositorioBase<AtividadeMedica>(context), IRepositorioAtividadeMedica
{
    public override async Task<List<AtividadeMedica>> SelecionarTodosAsync()
    {
        return await registros
            .Include(a => a.Medicos)
            .Include(a => a.Paciente)
            .ToListAsync();
    }

    public async Task<List<Consulta>> SelecionarConsultasAsync()
    {
        return await registros.OfType<Consulta>()
            .Include(c => c.Medicos)
            .Include(a => a.Paciente)
            .ToListAsync();
    }

    public async Task<List<Cirurgia>> SelecionarCirurgiasAsync()
    {
        return await registros.OfType<Cirurgia>()
            .Include(c => c.Medicos)
            .Include(a => a.Paciente)
            .ToListAsync();
    }

    public async Task<IEnumerable<AtividadeMedica>> SelecionarPorMedicosEPeriodo(
        IEnumerable<Guid> medicoIds,
        DateTime inicio,
        DateTime termino
    )
    {
        return await registros
            .Where(a => a.Medicos.Any(m => medicoIds.Contains(m.Id)) &&
                        a.Inicio < termino &&
                        (a.Termino ?? DateTime.MaxValue) > inicio)
            .ToListAsync();
    }

    public override async Task<AtividadeMedica?> SelecionarPorIdAsync(Guid id)
    {
        return await registros
            .Include(a => a.Medicos)
            .ThenInclude(m => m.Atividades)
            .Include(a => a.Paciente)
            .ThenInclude(p => p.Atividades)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<List<AtividadeMedica>> SelecionarAtividadesConfirmacaoPendenteAsync(
        DateTime periodoInicio,
        DateTime periodoTermino
    )
    {
        return await registros
            .IgnoreQueryFilters()
            .Include(a => a.Paciente)
            .Where(a => !a.ConfirmacaoEnviada && a.Inicio >= periodoInicio && a.Inicio <= periodoTermino)
            .ToListAsync();
    }
}