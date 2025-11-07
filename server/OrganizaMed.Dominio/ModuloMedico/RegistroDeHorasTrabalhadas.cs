namespace OrganizaMed.Dominio.ModuloMedico;

public class RegistroDeHorasTrabalhadas
{
    public required Guid MedicoId { get; set; }
    public required string Medico { get; set; }
    public required int TotalDeHorasTrabalhadas { get; set; }

    public RegistroDeHorasTrabalhadas() { }
}