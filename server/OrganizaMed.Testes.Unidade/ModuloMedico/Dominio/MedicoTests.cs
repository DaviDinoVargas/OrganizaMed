using FluentValidation.TestHelper;
using OrganizaMed.Dominio.ModuloMedico;

namespace OrganizaMed.Testes.Unidade.ModuloMedico.Dominio;

[TestClass]
[TestCategory("Testes de Unidade")]
public class MedicoTests
{
    private ValidadorMedico _validador;

    [TestInitialize]
    public void Inicializar()
    {
        _validador = new ValidadorMedico();
    }

    [TestMethod]
    public void Deve_Passar_Quando_Nome_E_Crm_Sao_Validos()
    {
        var medico = new Medico("João da Silva", "12345-SP");

        _validador.TestValidate(medico).ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void Deve_Falhar_Quando_Nome_Estiver_Vazio()
    {
        var medico = new Medico("", "12345-SP");

        var result = _validador.TestValidate(medico);

        result.ShouldHaveValidationErrorFor(m => m.Nome)
            .WithErrorMessage("O campo Nome é obrigatório");
    }

    [TestMethod]
    public void Deve_Falhar_Quando_Nome_For_Menor_Que_Tres_Caracteres()
    {
        var medico = new Medico("Ab", "12345-SP");

        var result = _validador.TestValidate(medico);

        result.ShouldHaveValidationErrorFor(m => m.Nome)
            .WithErrorMessage("O campo Nome deve conter no mínimo 3 caracteres");
    }

    [TestMethod]
    public void Deve_Falhar_Quando_Crm_Estiver_Vazio()
    {
        var medico = new Medico("João da Silva", "");

        var result = _validador.TestValidate(medico);

        result.ShouldHaveValidationErrorFor(m => m.Crm)
            .WithErrorMessage("O campo Crm é obrigatório");
    }

    [TestMethod]
    public void Deve_Falhar_Quando_Crm_Nao_Tiver_Formato_Correto()
    {
        var medico = new Medico("João da Silva", "1234-SP");

        var result = _validador.TestValidate(medico);

        result.ShouldHaveValidationErrorFor(m => m.Crm)
            .WithErrorMessage("O campo Crm deve seguir o formato 00000-UF");
    }

    [TestMethod]
    public void Deve_Falhar_Quando_Crm_Tiver_Letras_Minusculas_No_Estado()
    {
        var medico = new Medico("João da Silva", "12345-sp");

        var result = _validador.TestValidate(medico);

        result.ShouldHaveValidationErrorFor(m => m.Crm)
            .WithErrorMessage("O campo Crm deve seguir o formato 00000-UF");
    }
}
