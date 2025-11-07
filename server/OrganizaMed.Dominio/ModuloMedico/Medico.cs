using OrganizaMed.Dominio.Compartilhado;
using OrganizaMed.Dominio.ModuloAtividade;

namespace OrganizaMed.Dominio.ModuloMedico;

public class Medico : EntidadeBase
{
    public string Nome { get; set; }
    public string Crm { get; set; }
    public List<AtividadeMedica> Atividades { get; set; }

    protected Medico()
    {
        Atividades = [];
    }

    public Medico(string nome, string crm) : this()
    {
        Nome = nome;
        Crm = crm;
    }

    public bool RegistrarAtividade(AtividadeMedica atividade)
    {
        if (Atividades.Contains(atividade)) return false;

        Atividades.Add(atividade);

        return true;
    }

    public bool RemoverAtividade(AtividadeMedica atividade)
    {
        if (!Atividades.Contains(atividade)) return false;

        Atividades.Remove(atividade);

        return true;
    }

    public bool PeriodoDeDescansoEstaValido(AtividadeMedica atividade)
    {
        foreach (var atividadeRegistrada in Atividades)
        {
            if (atividadeRegistrada.Equals(atividade)) continue;

            TimeSpan diferencial;

            if (atividade.Inicio > atividadeRegistrada.Termino)
                diferencial = atividade.Inicio.Subtract(atividadeRegistrada.Termino.Value);
            else
                diferencial = atividadeRegistrada.Inicio.Subtract(atividade.Termino!.Value);

            if (diferencial <= atividadeRegistrada.ObterPeriodoDescanso())
                return false;
        }

        return true;
    }
}
