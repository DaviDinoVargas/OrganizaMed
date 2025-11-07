using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Editar;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Excluir;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Inserir;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarPorId;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.SelecionarTodos;
using OrganizaMed.WebApi.Extensions;

namespace OrganizaMed.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/pacientes")]
public class PacienteController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(InserirPacienteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Inserir(InserirPacienteRequest request)
    {
        var resultado = await mediator.Send(request);

        return resultado.ToHttpResponse();
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EditarPacienteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Editar(Guid id, EditarPacientePartialRequest request)
    {
        var editarRequest = new EditarPacienteRequest(
            id,
            request.Nome,
            request.Cpf,
            request.Email,
            request.Telefone
        );

        var resultado = await mediator.Send(editarRequest);

        return resultado.ToHttpResponse();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ExcluirPacienteResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Excluir(Guid id)
    {
        var excluirRequest = new ExcluirPacienteRequest(id);

        var resultado = await mediator.Send(excluirRequest);

        return resultado.ToHttpResponse();
    }

    [HttpGet]
    [ProducesResponseType(typeof(SelecionarPacientesResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SelecionarTodos()
    {
        var resultado = await mediator.Send(new SelecionarPacientesRequest());

        return resultado.ToHttpResponse();
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SelecionarPacientePorIdResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SelecionarPorId(Guid id)
    {
        var selecionarPorIdRequest = new SelecionarPacientePorIdRequest(id);

        var resultado = await mediator.Send(selecionarPorIdRequest);

        return resultado.ToHttpResponse();
    }
}