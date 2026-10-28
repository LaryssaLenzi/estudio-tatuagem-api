using System.ComponentModel.DataAnnotations;

namespace Tatuagem.Api.Dtos;

public record CriarTatuadorDto(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O estilo principal é obrigatório.")]
    string EstiloPrincipal,

    [Range(0.01, 100000.00, ErrorMessage = "O preço mínimo deve ser maior que zero.")]
    decimal PrecoMinimo
);

public record AtualizarTatuadorDto(
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    string Nome,

    [Required(ErrorMessage = "O estilo principal é obrigatório.")]
    string EstiloPrincipal,

    [Range(0.01, 100000.00, ErrorMessage = "O preço mínimo deve ser maior que zero.")]
    decimal PrecoMinimo
);

public record TatuadorResponseDto(
    int Id,
    string Nome,
    string EstiloPrincipal,
    decimal PrecoMinimo
);

public record PaginaResultadoDto<T>(
    List<T> Itens,
    int Pagina,
    int Tamanho,
    int Total,
    int TotalPaginas
);