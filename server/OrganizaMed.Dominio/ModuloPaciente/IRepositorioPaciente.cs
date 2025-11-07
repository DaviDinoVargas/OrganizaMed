namespace OrganizaMed.Dominio.ModuloPaciente;

public interface IRepositorioPaciente
{
    Task<Guid> InserirAsync(Paciente novaEntidade);
    Task<bool> EditarAsync(Paciente entidadeAtualizada);
    Task<bool> ExcluirAsync(Paciente entidadeParaRemover);
    Task<List<Paciente>> SelecionarTodosAsync();
    Task<Paciente?> SelecionarPorIdAsync(Guid id);
}