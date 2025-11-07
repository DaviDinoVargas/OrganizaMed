using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.ModuloMedico.Commands.Inserir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloMedico.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class InserirMedicoRequestHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioMedico> _repositorioMedicoMock;
    private Mock<IValidator<Medico>> _validadorMock;
    private Mock<ITenantProvider> _tenantProviderMock;

    private InserirMedicoRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioMedicoMock = new Mock<IRepositorioMedico>();
        _validadorMock = new Mock<IValidator<Medico>>();
        _tenantProviderMock = new Mock<ITenantProvider>();

        _handler = new InserirMedicoRequestHandler(
            _contextoMock.Object,
            _repositorioMedicoMock.Object,
            _tenantProviderMock.Object,
            _validadorMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Inserir_Medico()
    {
        // Arrange
        var request = new InserirMedicoRequest("João da Silva", "12345-SP");

        _validadorMock.Setup(v => v.ValidateAsync(It.IsAny<Medico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioMedicoMock
            .Setup(r => r.SelecionarTodosAsync())
            .ReturnsAsync(new List<Medico>());

        _repositorioMedicoMock
            .Setup(r => r.InserirAsync(It.IsAny<Medico>()))
            .ReturnsAsync(Guid.NewGuid());

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(x => x.InserirAsync(It.IsAny<Medico>()), Times.Once);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
    }
}
