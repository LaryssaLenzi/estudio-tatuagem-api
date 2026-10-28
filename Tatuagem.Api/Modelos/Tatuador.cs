namespace Tatuagem.Api.Modelos;

public class Tatuador
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string EstiloPrincipal { get; set; } = string.Empty;
    public decimal PrecoMinimo { get; set; }

    public ICollection<Sessao> Sessoes { get; set; } = new List<Sessao>();
}