using HidroRec.Backend.Domain.Enums;

namespace HidroRec.Backend.Domain.Entities;

public sealed class Alerta : BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Titulo { get; set; } = string.Empty;

    public string Descricao { get; set; } = string.Empty;

    public CriticidadeAlerta Criticidade { get; set; }

    public bool Ativo { get; set; } = true;

    public string AreaAfetada { get; set; } = string.Empty;

    public string OrientacaoResumida { get; set; } = string.Empty;

    public int? BairroId { get; set; }

    public Bairro? Bairro { get; set; }

    public ICollection<AlertaReporte> AlertaReportes { get; set; } = new List<AlertaReporte>();
}
