using OrganizaMed.Aplicacao.ModuloAtividade.DTOs;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.SelecionarTodos;

public record SelecionarAtividadesMedicasResponse
{
    public required int QuantidadeRegistros { get; init; }
    public required IEnumerable<SelecionarAtividadesDto> Registros { get; init; }
}