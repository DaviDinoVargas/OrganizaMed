using FluentResults;
using FluentValidation;
using MediatR;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Aplicacao.ModuloPaciente.Commands.Inserir;

public class InserirPacienteRequestHandler(
    IContextoPersistencia contexto,
    IRepositorioPaciente repositorioPaciente,
    ITenantProvider tenantProvider,
    IValidator<Paciente> validador
) : IRequestHandler<InserirPacienteRequest, Result<InserirPacienteResponse>>
{
    public async Task<Result<InserirPacienteResponse>> Handle(InserirPacienteRequest request, CancellationToken cancellationToken)
    {
        var paciente = new Paciente(request.Nome, request.Cpf, request.Email, request.Telefone)
        {
            UsuarioId = tenantProvider.UsuarioId.GetValueOrDefault()
        };

        // validações
        var resultadoValidacao = await validador.ValidateAsync(paciente);

        if (!resultadoValidacao.IsValid)
        {
            var erros = resultadoValidacao.Errors
                .Select(failure => failure.ErrorMessage)
                .ToList();

            return Result.Fail(ErrorResults.BadRequestError(erros));
        }

        var medicosRegistrados = await repositorioPaciente.SelecionarTodosAsync();

        if (CpfDuplicado(paciente, medicosRegistrados))
            return Result.Fail(PacienteErrorResults.CpfDuplicadoError(paciente.Nome));

        try
        {
            await repositorioPaciente.InserirAsync(paciente);

            await contexto.GravarAsync();
        }
        catch (Exception ex)
        {
            await contexto.RollbackAsync();

            return Result.Fail(ErrorResults.InternalServerError(ex));
        }

        return Result.Ok(new InserirPacienteResponse(paciente.Id));
    }
    
    private bool CpfDuplicado(Paciente paciente, IEnumerable<Paciente> pacientes)
    {
        return pacientes
            .Any(registro => string.Equals(
                registro.Cpf,
                paciente.Cpf,
                StringComparison.CurrentCultureIgnoreCase)
            );
    }
}