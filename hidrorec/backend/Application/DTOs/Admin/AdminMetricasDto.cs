namespace HidroRec.Backend.Application.DTOs.Admin;

public sealed class AdminMetricasDto
{
    public int TotalReportes { get; set; }

    public int Pendentes { get; set; }

    public int Confirmados { get; set; }

    public int AlertasAtivos { get; set; }

    public int UsuariosAtivos { get; set; }
}
