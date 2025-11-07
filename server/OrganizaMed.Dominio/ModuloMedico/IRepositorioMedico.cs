namespace OrganizaMed.Dominio.ModuloMedico;

public interface IRepositorioMedico
{
    Task<Guid> InserirAsync(Medico novaEntidade);
    Task<bool> EditarAsync(Medico entidadeAtualizada);
    Task<bool> ExcluirAsync(Medico entidadeParaRemover);
    Task<List<Medico>> SelecionarTodosAsync();
    Task<Medico?> SelecionarPorIdAsync(Guid id);
    Task<List<Medico>> SelecionarMuitosPorId(IEnumerable<Guid> requestMedicos);
    Task<List<RegistroDeHorasTrabalhadas>> SelecionarMedicosMaisAtivosPorPeriodo(
        DateTime inicioPeriodo, DateTime terminoPeriodo);
}