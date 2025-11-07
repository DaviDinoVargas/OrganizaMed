using FluentValidation;
using FluentValidation.Results;
using Moq;
using OrganizaMed.Aplicacao.Compartilhado;
using OrganizaMed.Aplicacao.ModuloAtividade;
using OrganizaMed.Aplicacao.ModuloAtividade.Commands.Inserir;
using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloAutenticacao;
using OrganizaMed.Dominio.ModuloMedico;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloAtividade.Aplicacao;

[TestClass]
[TestCategory("Testes de Unidade")]
public class InserirAtividadeMedicaRequestHandlerTests
{
    private Mock<IRepositorioAtividadeMedica> _repositorioAtividadeMedicaMock;
    private Mock<IRepositorioMedico> _repositorioMedicoMock;
    private Mock<IRepositorioPaciente> _repositorioPacienteMock;
    private Mock<IContextoPersistencia> _contextoMock;
    private Mock<IValidator<AtividadeMedica>> _validadorMock;
    private Mock<ITenantProvider> _tenantProviderMock;

    private InserirAtividadeMedicaRequestHandler _requestHandler;

    [TestInitialize]
    public void Inicializar()
    {
        _repositorioAtividadeMedicaMock = new Mock<IRepositorioAtividadeMedica>();
        _repositorioMedicoMock = new Mock<IRepositorioMedico>();
        _repositorioPacienteMock = new Mock<IRepositorioPaciente>();
        _contextoMock = new Mock<IContextoPersistencia>();
        _tenantProviderMock = new Mock<ITenantProvider>();
        _validadorMock = new Mock<IValidator<AtividadeMedica>>();

        _requestHandler = new InserirAtividadeMedicaRequestHandler(
            _repositorioAtividadeMedicaMock.Object,
            _repositorioMedicoMock.Object,
            _repositorioPacienteMock.Object,
            _contextoMock.Object,
            _tenantProviderMock.Object,
            _validadorMock.Object
        );
    }

    [TestMethod]
    public async Task Deve_Inserir_Consulta_Com_Sucesso()
    {
        // Arrange
        var paciente = new Paciente("João da Silva", "000.000.000-01", "joao@gmail.com", "(00) 00000-0000");
        
        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(paciente.Id))
            .ReturnsAsync(paciente);
        
        var request = new InserirAtividadeMedicaRequest(
            paciente.Id,
            DateTime.Now,
            DateTime.Now.AddHours(1),
            TipoAtividadeMedica.Consulta,
            new List<Guid> { Guid.NewGuid() }
        );

        var medico = new Medico("Dr. João", "12345-SP");
        
        _repositorioMedicoMock
            .Setup(r => r.SelecionarMuitosPorId(request.Medicos))
            .ReturnsAsync([medico]);

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioAtividadeMedicaMock
            .Setup(r => r.InserirAsync(It.IsAny<AtividadeMedica>()))
            .ReturnsAsync(Guid.NewGuid());

        _contextoMock
            .Setup(c => c.GravarAsync())
            .ReturnsAsync(1);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(x => x.InserirAsync(It.IsAny<AtividadeMedica>()), Times.Once);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Once);

        Assert.IsTrue(result.IsSuccess);
        Assert.IsNotNull(result.Value);
    }

    [TestMethod]
    public async Task Nao_Deve_Inserir_Quando_Medicos_Nao_Forem_Encontrados()
    {
        // Arrange
        var paciente = new Paciente("João da Silva", "000.000.000-01", "joao@gmail.com", "(00) 00000-0000");
        
        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(paciente.Id))
            .ReturnsAsync(paciente);
        
        var request = new InserirAtividadeMedicaRequest(
            paciente.Id,
            DateTime.Now,
            DateTime.Now.AddHours(1),
            TipoAtividadeMedica.Consulta,
            new List<Guid> { Guid.NewGuid() }
        );

        _repositorioMedicoMock
            .Setup(r => r.SelecionarMuitosPorId(request.Medicos))
            .ReturnsAsync([]);

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(x => x.InserirAsync(It.IsAny<AtividadeMedica>()), Times.Never);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Never);

        Assert.IsTrue(result.IsFailed);

        var mensagemErroEsperada = AtividadeMedicaErrorResults.MedicosNaoEncontradosError().Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Nao_Deve_Inserir_Atividade_Com_Erros_De_Validacao()
    {
        // Arrange
        var paciente = new Paciente("João da Silva", "000.000.000-01", "joao@gmail.com", "(00) 00000-0000");
        
        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(paciente.Id))
            .ReturnsAsync(paciente);
        
        var request = new InserirAtividadeMedicaRequest(
            paciente.Id,
            DateTime.Now.AddDays(-2),
            DateTime.Now.AddDays(-3),
            TipoAtividadeMedica.Consulta,
            new List<Guid> { Guid.NewGuid() }
        );

        var medico = new Medico("Dr. João", "12345-SP");

        _repositorioMedicoMock
            .Setup(r => r.SelecionarMuitosPorId(request.Medicos))
            .ReturnsAsync([medico]);

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new List<ValidationFailure>
            {
                new ValidationFailure("Inicio", "A data de início deve ser no presente ou no futuro"),
                new ValidationFailure("Termino", "A data de término deve ser posterior à data de início")
            }));

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _repositorioAtividadeMedicaMock.Verify(x => x.InserirAsync(It.IsAny<AtividadeMedica>()), Times.Never);
        _contextoMock.Verify(x => x.GravarAsync(), Times.Never);

        Assert.IsTrue(result.IsFailed);

        var mensagemErroEsperada = ErrorResults.BadRequestError(
            result.Errors.Select(e => e.Message).ToList()
        ).Message;

        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }

    [TestMethod]
    public async Task Deve_Retornar_Erro_Interno_Em_Caso_De_Exception()
    {
        // Arrange
        var paciente = new Paciente("João da Silva", "000.000.000-01", "joao@gmail.com", "(00) 00000-0000");
        
        _repositorioPacienteMock
            .Setup(r => r.SelecionarPorIdAsync(paciente.Id))
            .ReturnsAsync(paciente);
        
        var request = new InserirAtividadeMedicaRequest(
            paciente.Id,
            DateTime.Now,
            DateTime.Now.AddHours(2),
            TipoAtividadeMedica.Cirurgia,
            new List<Guid> { Guid.NewGuid() }
        );

        var medico = new Medico("Dr. João", "12345-SP");
        _repositorioMedicoMock
            .Setup(r => r.SelecionarMuitosPorId(request.Medicos))
            .ReturnsAsync([medico]);

        _validadorMock
            .Setup(v => v.ValidateAsync(It.IsAny<AtividadeMedica>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        _repositorioAtividadeMedicaMock
            .Setup(r => r.InserirAsync(It.IsAny<AtividadeMedica>()))
            .Throws(new Exception("Erro de banco de dados"));

        // Act
        var result = await _requestHandler.Handle(request, It.IsAny<CancellationToken>());

        // Assert
        _contextoMock.Verify(x => x.RollbackAsync(), Times.Once);

        Assert.IsFalse(result.IsSuccess);

        var mensagemErroEsperada = ErrorResults.InternalServerError(new Exception()).Message;
        Assert.AreEqual(mensagemErroEsperada, result.Errors.First().Message);
    }
}