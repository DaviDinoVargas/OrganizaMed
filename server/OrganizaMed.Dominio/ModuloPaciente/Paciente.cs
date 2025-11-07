using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Dominio.ModuloPaciente;

public class Paciente : EntidadeBase
{
    public string Nome { get; set; }
    public string Cpf { get; set; }
    public string Email { get; set; }
    public string Telefone { get; set; }
    public List<AtividadeMedica> Atividades { get; set; }

    protected Paciente()
    {
        Atividades = [];
    }

    public Paciente(string nome, string cpf, string email, string telefone): this()
    {
        Nome = nome;
        Cpf = cpf;
        Email = email;
        Telefone = telefone;
    }
}