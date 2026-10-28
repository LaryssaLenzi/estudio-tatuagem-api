using System.ComponentModel.DataAnnotations;

namespace Tatuagem.Api.Dtos;

public record CriarSessaoDto(
    [Required(ErrorMessage = "O ID do tatuador é obrigatório.")]
    int TatuadorId,

    [Required(ErrorMessage = "A data e hora são obrigatórias.")]
    DateTimeOffset DataHora,

    [Required(ErrorMessage = "A descrição da tatuagem é obrigatória.")]
    [StringLength(250, ErrorMessage = "A descrição deve ter no máximo 250 caracteres.")]
    string DescricaoTatuagem
);

public record SessaoResponseDto(
    int Id,
    int TatuadorId,
    string NomeTatuador,
    int UsuarioId,
    string NomeCliente,
    DateTime DataHoraUtc,
    string DescricaoTatuagem
);