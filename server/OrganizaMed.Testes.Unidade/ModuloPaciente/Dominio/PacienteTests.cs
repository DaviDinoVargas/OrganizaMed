using FluentValidation.TestHelper;
using OrganizaMed.Dominio.ModuloPaciente;

namespace OrganizaMed.Testes.Unidade.ModuloPaciente.Dominio
{
    [TestClass]
    [TestCategory("Testes de Unidade")]
    public class PacienteTests
    {
        private ValidadorPaciente _validador;

        [TestInitialize]
        public void Inicializar()
        {
            _validador = new ValidadorPaciente();
        }

        [TestMethod]
        public void Deve_Passar_Quando_Todas_Propriedades_Forem_Validas()
        {
            var paciente = new Paciente("João da Silva", "123.456.789-00", "joao.silva@email.com",
                "(11) 91234-5678");

            _validador.TestValidate(paciente).ShouldNotHaveAnyValidationErrors();
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Nome_Estiver_Vazio()
        {
            var paciente = new Paciente("", "123.456.789-00", "joao.silva@email.com",
                "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Nome)
                .WithErrorMessage("O campo Nome é obrigatório");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Nome_For_Menor_Que_Tres_Caracteres()
        {
            var paciente = new Paciente("Ab", "123.456.789-00", "joao.silva@email.com",
                "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Nome)
                .WithErrorMessage("O campo Nome deve conter no mínimo 3 caracteres");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Cpf_Estiver_Vazio()
        {
            var paciente = new Paciente("João da Silva", "", "joao.silva@email.com", "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Cpf)
                .WithErrorMessage("O campo Cpf é obrigatório");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Cpf_For_Invalido()
        {
            var paciente = new Paciente("João da Silva", "12345678900", // Sem o formato correto
                "joao.silva@email.com", "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Cpf)
                .WithErrorMessage("O campo Cpf deve seguir o formato 000.000.000-00");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Email_Estiver_Vazio()
        {
            var paciente = new Paciente("João da Silva", "123.456.789-00", "",
                "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Email)
                .WithErrorMessage("O campo Email é obrigatório");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Email_For_Invalido()
        {
            var paciente = new Paciente("João da Silva", "123.456.789-00", "joao.email.invalido",
                "(11) 91234-5678");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Email)
                .WithErrorMessage("O campo Email deve conter um endereço de e-mail válido");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Telefone_Estiver_Vazio()
        {
            var paciente = new Paciente("João da Silva", "123.456.789-00", "joao.silva@email.com",
                "");

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Telefone)
                .WithErrorMessage("O campo Telefone é obrigatório");
        }

        [TestMethod]
        public void Deve_Falhar_Quando_Telefone_For_Invalido()
        {
            var paciente = new Paciente("João da Silva", "123.456.789-00", "joao.silva@email.com",
                "11912345678" // Sem o formato correto
            );

            var result = _validador.TestValidate(paciente);

            result.ShouldHaveValidationErrorFor(p => p.Telefone)
                .WithErrorMessage("O campo Telefone deve seguir o formato (00) 00000-0000");
        }
    }
}
