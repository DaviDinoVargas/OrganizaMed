namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarTodos;

public record SelecionarPacientesDto(Guid Id, string Nome, string Cpf, string Email, string Telefone);

public record SelecionarPacientesResponse
{
    public required int QuantidadeRegistros { get; init; }
    public required IEnumerable<SelecionarPacientesDto> Registros { get; init; }
}