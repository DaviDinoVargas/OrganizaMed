using OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloAtividade.DTOs;

public record SelecionarAtividadesDto(
    Guid Id, 
    SelecionarPacienteAtividadeDto Paciente,
    DateTime Inicio,
    DateTime? Termino,
    TipoAtividadeMedica TipoAtividade,
    IEnumerable<SelecionarMedicosDto> Medicos
);