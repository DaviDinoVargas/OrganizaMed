using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Editar;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloPaciente.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class EditarPacienteRequestHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioPaciente> _repositorioPacienteMock;
    private Mock<IValidator<Paciente>> _validador;

    private EditarPacienteRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioPacienteMock = new Mock<IRepositorioPaciente>();
        _validador = new Mock<IValidator<Paciente>>();

        _handler = new EditarPacienteRequestHandler(
            _repositorioPacienteMock.Object,
            _contextoMock.Object,
            _validador.Object
        );
    }

    [TestMethod]
    public async Task Deve_Editar_Paciente_Com_Sucesso()
    {
        // Arrange
        var request = new EditarPacienteRequest(Guid.NewGuid(), "João da Silva",
            "000.000.000-00",
            "joao@email.com",
            "(00) 90000-0000"
        );

        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(new Paciente(
                "Antônio Carlos",
                "100.002.000-00", 
                "joao@email.com",
                "(00) 90000-0000") { Id = request.Id }
            );

        _validador
            .Setup(v => v.ValidateAsync(It.IsAny<Paciente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioPacienteMock
            .Setup(r => r.SelecionarTodosAsync())
            .ReturnsAsync(new List<Paciente>());

        _repositorioPacienteMock
            .Setup(r => r.EditarAsync(It.IsAny<Paciente>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(r => r.EditarAsync(It.IsAny<Paciente>()), Times.Once);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Nao_Deve_Editar_Paciente_Se_Nao_Encontrado()
    {
        // Arrange
        var request = new EditarPacienteRequest(Guid.NewGuid(), "João da Silva",
            "000.000.000-00",
            "joao@email.com",
            "(00) 90000-0000"
        );

        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync((Paciente)null);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(r => r.EditarAsync(It.IsAny<Paciente>()), Times.Never);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Never);

        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.NotFoundError(request.Id).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}