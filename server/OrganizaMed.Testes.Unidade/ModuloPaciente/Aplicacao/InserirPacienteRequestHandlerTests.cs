using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.ModuloPaciente.Commands.Inserir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloPaciente.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class InserirPacienteRequestHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioPaciente> _repositorioPacienteMock;
    private Mock<IValidator<Paciente>> _validadorMock;
    private Mock<ITenantProvider> _tenantProviderMock;

    private InserirPacienteRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioPacienteMock = new Mock<IRepositorioPaciente>();
        _validadorMock = new Mock<IValidator<Paciente>>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _handler = new InserirPacienteRequestHandler(
            _contextoMock.Object,
            _repositorioPacienteMock.Object,
            _tenantProviderMock.Object,
            _validadorMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Inserir_Paciente()
    {
        // Arrange
        var request = new InserirPacienteRequest(
            "João da Silva",
            "000.000.000-00",
            "joao@email.com",
            "(00) 90000-0000"
        );

        _validadorMock.Setup(v => v.ValidateAsync(It.IsAny<Paciente>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioPacienteMock
            .Setup(r => r.SelecionarTodosAsync())
            .ReturnsAsync(new List<Paciente>());

        _repositorioPacienteMock
            .Setup(r => r.InserirAsync(It.IsAny<Paciente>()))
            .ReturnsAsync(Guid.NewGuid());

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioPacienteMock.Verify(x => x.InserirAsync(It.IsAny<Paciente>()), Times.Once);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
    }
}