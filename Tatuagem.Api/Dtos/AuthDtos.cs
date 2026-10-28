using System.ComponentModel.DataAnnotations;

namespace Tatuagem.Api.Dtos;

public record RegistroDto(
    [Required(ErrorMessage = "O nome é obrigatório.")] string Nome,
    [Required(ErrorMessage = "O e-mail é obrigatório.")][EmailAddress(ErrorMessage = "E-mail inválido.")] string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")][MinLength(6, ErrorMessage = "A senha deve ter no mínimo 6 caracteres.")] string Senha
);

public record LoginDto(
    [Required(ErrorMessage = "O e-mail é obrigatório.")][EmailAddress(ErrorMessage = "E-mail inválido.")] string Email,
    [Required(ErrorMessage = "A senha é obrigatória.")] string Senha
);

public record UsuarioResponseDto(int Id, string Nome, string Email, string Papel);

public record LoginResponseDto(string Token, string Nome, string Email, string Papel);