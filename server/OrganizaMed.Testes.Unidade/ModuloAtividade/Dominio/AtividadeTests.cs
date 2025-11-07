using OrganizaMed.Dominio.ModuloAtividade;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloAtividade.Dominio;

[TestClass]
[TestCategory("Testes de Unidade")]
public class AtividadeTests
{
    [TestMethod]
    public void Deve_Registrar_Consulta_Com_Periodo_Descanso_Valido()
    {
        // Arrange
        var medico = new Medico("João da Silva", "00000-SC");

        var dataInicio = DateTime.Today + new TimeSpan(14, 0, 0);
        var dataTermino = DateTime.Today + new TimeSpan(16, 0, 0);

        var consulta = new Consulta(dataInicio, dataTermino, medico);

        var dataInicioSegundaConsulta = DateTime.Today + new TimeSpan(16, 15, 0);
        var dataTerminoSegundaConsulta = DateTime.Today + new TimeSpan(18, 15, 0);

        // Act
        var segundaConsulta = new Consulta(dataInicioSegundaConsulta, dataTerminoSegundaConsulta, medico);

        // Assert
        var errosValidacao = new ValidadorAtividadeMedica().Validate(segundaConsulta);
        Assert.AreEqual(0, errosValidacao.Errors.Count);
    }

    [TestMethod]
    public void Nao_Deve_Registrar_Consulta_Com_Periodo_Descanso_Invalido()
    {
        // Arrange
        var medico = new Medico("João da Silva", "00000-SC");

        var dataInicio = DateTime.Today + new TimeSpan(14, 0, 0);
        var dataTermino = DateTime.Today + new TimeSpan(16, 0, 0);
        var consulta = new Consulta(dataInicio, dataTermino, medico);

        var dataInicioSegundaConsulta = DateTime.Today + new TimeSpan(16, 10, 0);
        var dataTerminoSegundaConsulta = DateTime.Today + new TimeSpan(18, 10, 0);

        var segundaConsulta = new Consulta(dataInicioSegundaConsulta, dataTerminoSegundaConsulta, medico);

        // Act
        var errosValidacao = new ValidadorAtividadeMedica().Validate(segundaConsulta);

        // Assert
        var mensagenErroEsperada = "O médico 'João da Silva' está em período de descanso obrigatório";

        var mensagemErroRecebida = errosValidacao.Errors.First().ErrorMessage;

        Assert.AreEqual(1, errosValidacao.Errors.Count);
        Assert.AreEqual(mensagenErroEsperada, mensagemErroRecebida);
    }

    [TestMethod]
    public void Nao_Deve_Registrar_Consulta_Com_Mais_De_Um_Medico()
    {
        // Arrange
        var medico = new Medico("João da Silva", "00000-SC");
        var segundoMedico = new Medico("Julia Santos", "10002-SP");

        var dataInicio = DateTime.Today + new TimeSpan(14, 0, 0);
        var dataTermino = DateTime.Today + new TimeSpan(16, 0, 0);

        var consulta = new Consulta(dataInicio, dataTermino, medico);

        consulta.AdicionarMedico(segundoMedico);
        // Act
        var errosValidacao = new ValidadorAtividadeMedica().Validate(consulta);

        // Assert
        var mensagemEsperada = "Consultas só podem ser realizadas por um médico";

        Assert.AreEqual(1, errosValidacao.Errors.Count);
        Assert.AreEqual(mensagemEsperada, errosValidacao.Errors.First().ErrorMessage);
    }

    [TestMethod]
    public void Deve_Registrar_Cirurgia_Com_Periodo_Descanso_Valido()
    {
        // Arrange

        List<Medico> medicos = [
             new Medico("João da Silva", "00000-SC"),
             new Medico("Julia Santos", "10002-SP")
        ];

        var dataInicio = DateTime.Today + new TimeSpan(14, 0, 0);
        var dataTermino = DateTime.Today + new TimeSpan(16, 0, 0);

        var cirurgia = new Cirurgia(dataInicio, dataTermino, medicos);

        var dataInicioSegundaCirurgia = DateTime.Today + new TimeSpan(20, 01, 0);
        var dataTerminoSegundaCirurgia = DateTime.Today + new TimeSpan(21, 01, 0);

        // Act
        var segundaCirurgia = new Cirurgia(
            dataInicioSegundaCirurgia,
            dataTerminoSegundaCirurgia,
            medicos
        );

        // Assert
        var errosValidacao = new ValidadorAtividadeMedica().Validate(segundaCirurgia);
        Assert.AreEqual(0, errosValidacao.Errors.Count);
    }

    [TestMethod]
    public void Nao_Deve_Registrar_Cirurgia_Com_Periodo_Descanso_Invalido()
    {
        // Arrange

        List<Medico> medicos = [
             new Medico("João da Silva", "00000-SC"),
             new Medico("Júlia Santos", "10002-SP")
        ];

        var dataInicio = DateTime.Today + new TimeSpan(14, 0, 0);
        var dataTermino = DateTime.Today + new TimeSpan(16, 0, 0);

        var cirurgia = new Cirurgia(dataInicio, dataTermino, medicos);

        var dataInicioSegundaCirurgia = DateTime.Today + new TimeSpan(20, 00, 0);
        var dataTerminoSegundaCirurgia = DateTime.Today + new TimeSpan(21, 01, 0);

        // Act
        var segundaCirurgia = new Cirurgia(
            dataInicioSegundaCirurgia,
            dataTerminoSegundaCirurgia,
            medicos
        );

        // Assert
        var errosValidacao = new ValidadorAtividadeMedica().Validate(segundaCirurgia);

        var mensagensErroEsperadas = new List<string> {
            "O médico 'João da Silva' está em período de descanso obrigatório",
            "O médico 'Júlia Santos' está em período de descanso obrigatório"
        };

        var mensagensErroRecebidas = errosValidacao.Errors
            .Select(e => e.ErrorMessage).ToList();

        Assert.AreEqual(2, errosValidacao.Errors.Count);
        CollectionAssert.AreEqual(mensagensErroEsperadas, mensagensErroRecebidas);
    }
}