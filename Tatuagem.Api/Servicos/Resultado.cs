namespace Tatuagem.Api.Servicos;

public enum TipoErro
{
    Validacao,
    NaoEncontrado,
    Proibido,
    Conflito
}

public class Resultado<T>
{
    public bool Sucesso { get; private set; }
    public T? Valor { get; private set; }
    public TipoErro? Erro { get; private set; }
    public string MensagemErro { get; private set; } = string.Empty;

    public static Resultado<T> Ok(T valor) => new() { Sucesso = true, Valor = valor };
    public static Resultado<T> Falha(TipoErro erro, string mensagem) => new() { Sucesso = false, Erro = erro, MensagemErro = mensagem };
}