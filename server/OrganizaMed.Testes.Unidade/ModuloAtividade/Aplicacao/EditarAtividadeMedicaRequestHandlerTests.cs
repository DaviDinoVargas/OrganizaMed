using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloAtividade.Commands.Editar;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloAtividade.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class EditarAtividadeMedicaRequestHandlerTests
{
    private Mock<IRepositorioAtividadeMedica> _repositorioAtividadeMedicaMock;
    private Mock<IRepositorioMedico> _repositorioMedicoMock;
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IValidator<AtividadeMedica>> _validadorMock;

    private EditarAtividadeMedicaRequestHandler _requestHandler;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioAtividadeMedicaMock = new Mock<IRepositorioAtividadeMedica>();
        _repositorioMedicoMock = new Mock<IRepositorioMedico>();
        _contextoMock = new Mock<IContextoPersistencia>();
        _validadorMock = new Mock<IValidator<AtividadeMedica>>();

        _requestHandler = new EditarAtividadeMedicaRequestHandler(
            _repositorioAtividadeMedicaMock.Object,
            _repositorioMedicoMock.Object,
            _contextoMock.Object,
            _validadorMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Editar_Atividade_Com_Sucesso()
    {
        // Arrange
        var request = new EditarAtividadeMedicaRequest(
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddHours(1),
            new List<Guid> { Guid.NewGuid() }
        );

        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(2), new Medico("Dr. João", "12345-SP"));
        var medicoAdicionado = new Medico("Dr. Maria", "67890-SP");
        var medicoRemovido = atividade.Medicos.First();

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(atividade);

        _repositorioMedicoMock
            .Setup(r => r.SelecionarMuitosPorId(request.Medicos))
            .ReturnsAsync(new List<Medico> { medicoAdicionado });

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioAtividadeMedicaMock
            .Setup(r => r.EditarAsync(It.IsAny<AtividadeMedica>()))
            .ReturnsAsync(true);

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(x => x.EditarAsync(It.IsAny<AtividadeMedica>()), Times.Once);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
        Assert.AreEqual(atividade.Id, result.Value.Id);
    }

    [TestMethod]
    public async Task Nao_Deve_Editar_Atividade_Nao_Encontrada()
    {
        // Arrange
        var request = new EditarAtividadeMedicaRequest(
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddHours(1),
            null
        );

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync((AtividadeMedica)null);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(ErrorResults.NotFoundError(request.Id).Message, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Nao_Deve_Editar_Atividade_Com_Erros_De_Validacao()
    {
        // Arrange
        var request = new EditarAtividadeMedicaRequest(
            Guid.NewGuid(),
            DateTime.Now.AddDays(-1),
            DateTime.Now.AddDays(-2),
            null
        );

        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(2), new Medico("Dr. João", "12345-SP"));

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(atividade);

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Termino", "A data de término deve ser posterior à data de início")
            }));

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        Assert.IsTrue(result.IsFailed);
        Assert.AreEqual(1, result.Errors.Count);
    }

    [TestMethod]
    public async Task Deve_Retornar_Erro_Interno_Em_Caso_De_Exception()
    {
        // Arrange
        var request = new EditarAtividadeMedicaRequest(
            Guid.NewGuid(),
            DateTime.Now,
            DateTime.Now.AddHours(2),
            null
        );

        var atividade = new Consulta(DateTime.Now, DateTime.Now.AddHours(2), new Medico("Dr. João", "12345-SP"));

        _repositorioAtividadeMedicaMock
            .Setup(r => r.SelecionarPorIdAsync(request.Id))
            .ReturnsAsync(atividade);

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioAtividadeMedicaMock
            .Setup(r => r.EditarAsync(It.IsAny<AtividadeMedica>()))
            .Throws(new Exception("Erro de banco de dados"));

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _contextoMock.Verify(x => x.RollbackAsync(), Times.Once);

        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual(ErrorResults.InternalServerError(new Exception()).Message, result.Errors.First().Message);
    }
}
