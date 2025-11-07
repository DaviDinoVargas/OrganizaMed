namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;

public record SelecionarRegistroDeHorasTrabalhadasDto(
    Guid MedicoId, string Medico, int TotalDeHorasTrabalhadas);

public record SelecionarMedicosMaisAtivosResponse
{
    public required int QuantidadeRegistros { get; init; }
    public required IEnumerable<SelecionarRegistroDeHorasTrabalhadasDto> Registros { get; init; }
}
