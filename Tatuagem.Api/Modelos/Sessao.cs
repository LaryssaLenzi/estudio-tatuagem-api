namespace Tatuagem.Api.Modelos;

public class Sessao
{
    public int Id { get; set; }
    public int TatuadorId { get; set; }
    public Tatuador? Tatuador { get; set; }

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public DateTime DataHoraUtc { get; set; }
    public string DescricaoTatuagem { get; set; } = string.Empty;
}