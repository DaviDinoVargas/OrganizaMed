using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloMedico.Commands.Excluir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloMedico;


namespace OrganizaMed.Testes.Unidade.ModuloMedico.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class ExcluirMedicoHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioMedico> _repositorioMedicoMock;

    private ExcluirMedicoRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioMedicoMock = new Mock<IRepositorioMedico>();
        
        _handler = new ExcluirMedicoRequestHandler(
            _repositorioMedicoMock.Object, 
            _contextoMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Excluir_Medico_Com_Sucesso()
    {
        // Arrange
        var request = new ExcluirMedicoRequest(Guid.NewGuid());
        var medicoSelecionado = new Medico("João da Silva", "12345-SP")
        {
            Id = request.Id
        };
        
        _repositorioMedicoMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(medicoSelecionado);

        _repositorioMedicoMock
            .Setup(r => r.ExcluirAsync(It.IsAny<Medico>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(r => r.ExcluirAsync(It.IsAny<Medico>()), Times.Once);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Once);
        
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Nao_Deve_Excluir_Medico_Se_Nao_Encontrado()
    {
        // Arrange
        var request = new ExcluirMedicoRequest(Guid.NewGuid());

        _repositorioMedicoMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync((Medico)null);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(r => r.ExcluirAsync(It.IsAny<Medico>()), Times.Never);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Never);
        
        Assert.IsFalse(result.IsSuccess);
        
        var mensagemErroEsperada = ErrorResults.NotFoundError(request.Id).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Deve_Retornar_Erro_Se_Ocorreu_Excecao_Ao_Excluir()
    {
        // Arrange
        var request = new ExcluirMedicoRequest(Guid.NewGuid());

        _repositorioMedicoMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(new Medico("João da Silva", "12345-SP") { Id = request.Id });

        _repositorioMedicoMock
            .Setup(r => r.ExcluirAsync(It.IsAny<Medico>()))
            .ThrowsAsync(new Exception("Erro ao excluir médico"));

        _contextoMock
            .Setup(c => c.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(r => r.ExcluirAsync(It.IsAny<Medico>()), Times.Once);
        _contextoMock.Verify(c => c.RollbackAsync(), Times.Once);
        
        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.InternalServerError(new Exception()).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}
