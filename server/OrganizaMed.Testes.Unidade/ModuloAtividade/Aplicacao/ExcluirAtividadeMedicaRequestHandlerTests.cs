using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloAtividade.Commands.Excluir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloAtividade.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class ExcluirAtividadeMedicaRequestHandlerTests
{
    private Mock<IRepositorioAtividadeMedica> _repositorioAtividadeMedicaMock;
    private Mock<IContextoPersistencia> _contextoMock;

    private ExcluirAtividadeMedicaRequestHandler _requestHandler;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioAtividadeMedicaMock = new Mock<IRepositorioAtividadeMedica>();
        _contextoMock = new Mock<IContextoPersistencia>();

        _requestHandler = new ExcluirAtividadeMedicaRequestHandler(
            _repositorioAtividadeMedicaMock.Object,
            _contextoMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Excluir_Atividade_Com_Sucesso()
    {
        // Arrange
        var atividadeId = Guid.NewGuid();
        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(1), new Medico("Dr. João", "12345-SP"));

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(atividadeId))
            .ReturnsAsync(atividade);

        _repositorioAtividadeMedicaMock
            .Setup(r => r.ExcluirAsync(It.IsAny<AtividadeMedica>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        var request = new ExcluirAtividadeMedicaRequest(atividadeId);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(r => r.ExcluirAsync(atividade), Times.Once);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Nao_Deve_Excluir_Quando_Atividade_Nao_Encontrada()
    {
        // Arrange
        var atividadeId = Guid.NewGuid();

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(atividadeId))
            .ReturnsAsync((AtividadeMedica)null);

        var request = new ExcluirAtividadeMedicaRequest(atividadeId);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(r => r.ExcluirAsync(It.IsAny<AtividadeMedica>()), Times.Never);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Never);

        Assert.IsTrue(result.IsFailed);

        var mensagemErroEsperada = ErrorResults.NotFoundError(atividadeId).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Deve_Retornar_Erro_Interno_Em_Caso_De_Exception_Durante_Exclusao()
    {
        // Arrange
        var atividadeId = Guid.NewGuid();
        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(1), new Medico("Dr. João", "12345-SP"));

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(atividadeId))
            .ReturnsAsync(atividade);

        _repositorioAtividadeMedicaMock
            .Setup(r => r.ExcluirAsync(It.IsAny<AtividadeMedica>()))
            .Throws(new Exception("Erro inesperado"));

        var request = new ExcluirAtividadeMedicaRequest(atividadeId);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _contextoMock.Verify(c => c.RollbackAsync(), Times.Once);

        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.InternalServerError(new Exception("Erro inesperado")).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}
