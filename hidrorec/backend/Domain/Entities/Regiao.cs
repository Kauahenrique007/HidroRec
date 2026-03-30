namespace HidroRec.Backend.Domain.Entities;

public sealed class Regiao : BaseEntity
{
    public int Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public ICollection<Bairro> Bairros { get; set; } = new List<Bairro>();
}
