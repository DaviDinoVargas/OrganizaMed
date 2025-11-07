using OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarPorId;

public record SelecionarAtividadePacienteDto(
    Guid Id,
    DateTime Inicio,
    DateTime Termino,
    TipoAtividadeMedica TipoAtividade,
    IEnumerable<SelecionarMedicosDto> Medicos

); 

public record SelecionarPacientePorIdResponse(
    Guid Id,
    string Nome,
    string Cpf,
    string Email,
    string Telefone,
    IEnumerable<SelecionarAtividadePacienteDto> Atividades
);