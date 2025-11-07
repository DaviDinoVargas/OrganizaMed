namespace OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;

public record SelecionarMedicosDto(Guid Id, string Nome, string Crm);

public record SelecionarMedicosResponse
{
    public required int QuantidadeRegistros { get; init; }
    public required IEnumerable<SelecionarMedicosDto> Registros { get; init; }
}
