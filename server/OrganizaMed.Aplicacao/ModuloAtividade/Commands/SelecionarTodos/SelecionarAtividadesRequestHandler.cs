using FluentResults;
using MediatR;
using OrganizaMed.Aplicacao.ModuloAtividade.DTOs;
using OrganizaMed.Aplicacao.ModuloMedico.Commands.SelecionarTodos;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.SelecionarTodos;

public class SelecionarAtividadesRequestHandler(
    IRepositorioAtividadeMedica repositorioAtividadeMedica
) : IRequestHandler<SelecionarAtividadesMedicasRequest, Result<SelecionarAtividadesMedicasResponse>>
{
    public async Task<Result<SelecionarAtividadesMedicasResponse>> Handle(
        SelecionarAtividadesMedicasRequest request, CancellationToken cancellationToken)
    {
        IEnumerable<AtividadeMedica> registrosFiltrados;
        
        switch (request.TipoAtividade)
        {
            case TipoAtividadeMedica.Consulta:
                registrosFiltrados = await repositorioAtividadeMedica.SelecionarConsultasAsync();
                break;
            case TipoAtividadeMedica.Cirurgia:
                registrosFiltrados = await repositorioAtividadeMedica.SelecionarCirurgiasAsync();
                break;
            default:
                registrosFiltrados = await repositorioAtividadeMedica.SelecionarTodosAsync();
                break;
        }

        var response = new SelecionarAtividadesMedicasResponse
        {
            QuantidadeRegistros = registrosFiltrados.Count(),
            Registros = registrosFiltrados.Select(a => new SelecionarAtividadesDto(
                a.Id,
                new SelecionarPacienteAtividadeDto(
                    a.PacienteId,
                    a.Paciente.Nome, 
                    a.Paciente.Email,
                    a.Paciente.Telefone
                ),
                a.Inicio,
                a.Termino,
                a.TipoAtividade,
                a.Medicos.Select(m => new SelecionarMedicosDto(m.Id, m.Nome, m.Crm)))
            )
        };

        return Result.Ok(response);
    }
}