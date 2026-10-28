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
}