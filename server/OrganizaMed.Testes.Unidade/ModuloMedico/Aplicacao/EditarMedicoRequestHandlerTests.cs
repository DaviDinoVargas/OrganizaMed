using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloMedico.Commands.Editar;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloMedico.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class EditarMedicoHandlerTests
{
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IRepositorioMedico> _repositorioMedicoMock;
    private Mock<IValidator<Medico>> _validador;

    private EditarMedicoRequestHandler _handler;

    [TestInitialize]
    public void Inicializar()
    {
        _contextoMock = new Mock<IContextoPersistencia>();
        _repositorioMedicoMock = new Mock<IRepositorioMedico>();
        _validador = new Mock<IValidator<Medico>>();

        _handler = new EditarMedicoRequestHandler(
            _repositorioMedicoMock.Object,
            _contextoMock.Object,
            _validador.Object
        );
    }

    [TestMethod]
    public async Task Deve_Editar_Medico_Com_Sucesso()
    {
        // Arrange
        var request = new EditarMedicoRequest(Guid.NewGuid(), "João da Silva", "12345-SP");

        _repositorioMedicoMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(new Medico("Antônio Carlos", "67890-SP") { Id = request.Id });

        _validador
            .Setup(v => v.ValidateAsync(It.IsAny<Medico>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioMedicoMock
            .Setup(r => r.SelecionarTodosAsync())
            .ReturnsAsync(new List<Medico>());

        _repositorioMedicoMock
            .Setup(r => r.EditarAsync(It.IsAny<Medico>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(r => r.EditarAsync(It.IsAny<Medico>()), Times.Once);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
    }

    [TestMethod]
    public async Task Nao_Deve_Editar_Medico_Se_Nao_Encontrado()
    {
        // Arrange
        var request = new EditarMedicoRequest(Guid.NewGuid(), "João da Silva", "12345-SP");

        _repositorioMedicoMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync((Medico)null);

        // Act
        var result = await _handler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioMedicoMock.Verify(r => r.EditarAsync(It.IsAny<Medico>()), Times.Never);
        _contextoMock.Verify(c => c.GravarAsync(), Times.Never);

        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.NotFoundError(request.Id).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}
