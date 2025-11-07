namespace OrganizaMed.Dominio.ModuloAtividade;

public interface IRepositorioAtividadeMedica
{
    Task<Guid> InserirAsync(AtividadeMedica novaEntidade);
    Task<bool> EditarAsync(AtividadeMedica entidadeAtualizada);
    Task<bool> ExcluirAsync(AtividadeMedica entidadeParaRemover);
    Task<AtividadeMedica?> SelecionarPorIdAsync(Guid id);
    Task<List<AtividadeMedica>> SelecionarTodosAsync();
    Task<List<Consulta>> SelecionarConsultasAsync();
    Task<List<Cirurgia>> SelecionarCirurgiasAsync();
    Task<IEnumerable<AtividadeMedica>> SelecionarPorMedicosEPeriodo(
        IEnumerable<Guid> medicoIds,
        DateTime inicio,
        DateTime termino
    );
    Task<List<AtividadeMedica>> SelecionarAtividadesConfirmacaoPendenteAsync(DateTime periodoInicio, DateTime periodoTermino);
}