namespace HidroRec.Backend.Domain.Entities;

public abstract class BaseEntity
{
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime DataAtualizacao { get; set; } = DateTime.UtcNow;

    public bool Excluido { get; set; }
}
