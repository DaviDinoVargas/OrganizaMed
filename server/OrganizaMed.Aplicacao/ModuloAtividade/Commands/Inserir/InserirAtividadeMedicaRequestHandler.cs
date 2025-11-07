using FluentResults;
using FluentValidation;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloMedico;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Aplicacao.ModuloAtividade.Commands.Inserir;

public class InserirAtividadeMedicaRequestHandler(
    IRepositorioAtividadeMedica repositorioAtividadeMedica,
    IRepositorioMedico repositorioMedico,
    IRepositorioPaciente repositorioPaciente,
    IContextoPersistencia contexto,
    ITenantProvider tenantProvider,
    IValidator<AtividadeMedica> validador
) : IRequestHandler<InserirAtividadeMedicaRequest, Result<InserirAtividadeMedicaResponse>>
{
    public async Task<Result<InserirAtividadeMedicaResponse>> Handle(
        InserirAtividadeMedicaRequest request, CancellationToken cancellationToken)
    {
        var medicosSelecionados = await repositorioMedico.SelecionarMuitosPorId(request.Medicos);

        if (medicosSelecionados.Count == 0)
            return Result.Fail(AtividadeMedicaErrorResults.MedicosNaoEncontradosError());

        var pacienteSelecionado = await repositorioPaciente.SelecionarPorIdAsync(request.PacienteId);

        if (pacienteSelecionado is null)
            return Result.Fail(AtividadeMedicaErrorResults.PacienteNaoEncontradoError());

        AtividadeMedica atividade;

        if (request.TipoAtividade == TipoAtividadeMedica.Consulta)
        {
            atividade = new Consulta(
                inicio: request.Inicio,
                termino: request.Termino,
                medico: medicosSelecionados.First()
            );
        }
        else
        {
            atividade = new Cirurgia(
                inicio: request.Inicio,
                termino: request.Termino,
                medicos: medicosSelecionados
            );
        }

        atividade.PacienteId = pacienteSelecionado.Id;
        atividade.UsuarioId = tenantProvider.UsuarioId.GetValueOrDefault();

        var resultadoValidacao =
            await validador.ValidateAsync(atividade, cancellationToken);

        if (!resultadoValidacao.IsValid)
        {
            var erros = resultadoValidacao.Errors
                .Select(failure => failure.ErrorMessage)
                .ToList();

            return Result.Fail(ErrorResults.BadRequestError(erros));
        }

        try
        {
            await repositorioAtividadeMedica.InserirAsync(atividade);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();

            return Result.Fail(ErrorResults.InternalServerError(ex));
        }

        return Result.Ok(new InserirAtividadeMedicaResponse(atividade.Id));
    }
}