using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloAtividade.Commands.SelecionarPorId;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloMedico;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloAtividade.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class SelecionarAtividadeMedicaPorIdRequestHandlerTests
{
    private Mock<IRepositorioAtividadeMedica> _repositorioAtividadeMedicaMock;
    private SelecionarAtividadeMedicaPorIdRequestHandler _requestHandler;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioAtividadeMedicaMock = new Mock<IRepositorioAtividadeMedica>();
        _requestHandler = new SelecionarAtividadeMedicaPorIdRequestHandler(_repositorioAtividadeMedicaMock.Object);
    }

    [TestMethod]
    public async Task Deve_Retornar_AtividadeMedica_Com_Sucesso()
    {
        // Arrange
        var medico = new Medico("Dr. João", "12345-SP");
        var paciente = new Paciente("João da Silva", "123.456.789-00", "joao.silva@email.com", "(11) 91234-5678");

        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(1), medico);

        atividade.PacienteId = paciente.Id;
        atividade.Paciente = paciente;

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(atividade.Id))
            .ReturnsAsync(atividade);

        var request = new SelecionarAtividadeMedicaPorIdRequest(atividade.Id);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(r => r.SelecionarPorIdAsync(atividade.Id), Times.Once);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);

        var dto = result.Value;

        Assert.AreEqual(atividade.Id, dto.Id);
        Assert.AreEqual(atividade.Inicio, dto.Inicio);
        Assert.AreEqual(atividade.Termino, dto.Termino);
        Assert.AreEqual(atividade.TipoAtividade, dto.TipoAtividade);
        Assert.AreEqual(1, dto.Medicos.Count());

        var medicoDtos = dto.Medicos.ToList();

        for (int i = 0; i < medicoDtos.Count; i++)
        {
            Assert.AreEqual(medico.Id, medicoDtos[i].Id);
            Assert.AreEqual(medico.Nome, medicoDtos[i].Nome);
            Assert.AreEqual(medico.Crm, medicoDtos[i].Crm);
        }
    }

    [TestMethod]
    public async Task Deve_Retornar_NotFound_Quando_Atividade_Nao_Existir()
    {
        // Arrange
        var atividadeId = Guid.NewGuid();

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(atividadeId))
            .ReturnsAsync((AtividadeMedica)null);

        var request = new SelecionarAtividadeMedicaPorIdRequest(atividadeId);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(r => r.SelecionarPorIdAsync(atividadeId), Times.Once);

        Assert.IsTrue(result.IsFailed);

        var mensagemErroEsperada = ErrorResults.NotFoundError(atividadeId).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}
