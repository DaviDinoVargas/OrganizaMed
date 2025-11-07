using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Excluir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloPaciente.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class ExcluirPacienteRequesHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioPaciente> _repositorioPacienteMock;

    private ExcluirPacienteRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioPacienteMock = new Mock<IRepositorioPaciente>();
        
        _handler = new ExcluirPacienteRequestHandler(
            _repositorioPacienteMock.Object, 
            _contextoMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Excluir_Paciente_Com_Sucesso()
    {
        // Arrange
        var request = new ExcluirPacienteRequest(Guid.NewGuid());
        var medicoSelecionado = new Paciente("João da Silva", "000.000.000-00", "joao@email.com", "(00) 00000-0000")
        {
            Id = request.Id
        };
        
        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(medicoSelecionado);

        _repositorioPacienteMock
            .Setup(r => r.ExcluirAsync(It.IsAny<Paciente>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(r => r.ExcluirAsync(It.IsAny<Paciente>()), Times.Once);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Once);
        
        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Nao_Deve_Excluir_Paciente_Se_Nao_Encontrado()
    {
        // Arrange
        var request = new ExcluirPacienteRequest(Guid.NewGuid());

        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync((Paciente)null);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(r => r.ExcluirAsync(It.IsAny<Paciente>()), Times.Never);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Never);
        
        Assert.IsFalse(result.IsSuccess);
        
        var mensagemErroEsperada = ErrorResults.NotFoundError(request.Id).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Deve_Retornar_Erro_Se_Ocorreu_Excecao_Ao_Excluir()
    {
        // Arrange
        var request = new ExcluirPacienteRequest(Guid.NewGuid());

        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(new Paciente(
                "João da Silva",
                "000.000.000-00",
                "joao@email.com", 
                "(00) 00000-0000") { Id = request.Id }
            );

        _repositorioPacienteMock
            .Setup(r => r.ExcluirAsync(It.IsAny<Paciente>()))
            .ThrowsAsync(new Exception("Erro ao excluir médico"));

        _contextoMock
            .Setup(c => c.RollbackAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(r => r.ExcluirAsync(It.IsAny<Paciente>()), Times.Once);
        _contextoMock.Verify(c => c.RollbackAsync(), Times.Once);
        
        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.InternalServerError(new Exception()).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}