using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Servicos;

namespace Tatuagem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthServico _authServico;

    public AuthController(AuthServico authServico)
    {
        _authServico = authServico;
    }

    /// <summary>
    /// Registra um novo cliente no sistema
    /// </summary>
    [HttpPost("registro")]
    [ProducesResponseType(typeof(UsuarioResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] RegistroDto dto)
    {
        var resultado = await _authServico.RegistrarAsync(dto);

        if (!resultado.Sucesso)
        {
            return resultado.Erro == TipoErro.Conflito
                ? Problem(statusCode: 409, detail: resultado.MensagemErro)
                : Problem(statusCode: 400, detail: resultado.MensagemErro);
        }

        return Created("", resultado.Valor);
    }

    /// <summary>
    /// Realiza a autenticação e retorna o Token JWT
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var resultado = await _authServico.LoginAsync(dto);

        if (!resultado.Sucesso)
        {
            return Problem(statusCode: 400, detail: resultado.MensagemErro);
        }

        return Ok(resultado.Valor);
    }

    /// <summary>
    /// Altera a senha do usuário autenticado (requer token JWT)
    /// </summary>
    [HttpPut("me/senha")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AlterarSenha([FromBody] AlterarSenhaDto dto)
    {
        // Obtém o ID do usuário através da Claim de identificação do token JWT
        var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
                   ?? User.FindFirst("sub")?.Value 
                   ?? User.FindFirst("id")?.Value;

        if (!int.TryParse(claimId, out var usuarioId))
        {
            return Unauthorized(new { detail = "Identificação do usuário inválida no token." });
        }

        var resultado = await _authServico.AlterarSenhaAsync(usuarioId, dto);

        if (!resultado.Sucesso)
        {
            return resultado.Erro == TipoErro.NaoEncontrado
                ? Problem(statusCode: 404, detail: resultado.MensagemErro)
                : Problem(statusCode: 400, detail: resultado.MensagemErro);
        }

        return Ok(new { message = "Senha alterada com sucesso!" });
    }
}