using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Servicos;

namespace Tatuagem.Api.Controllers;

[ApiController]
[Route("api/sessoes")]
[Authorize]
public class SessaoController : ControllerBase
{
    private readonly SessaoServico _sessaoServico;

    public SessaoController(SessaoServico sessaoServico)
    {
        _sessaoServico = sessaoServico;
    }

    [HttpPost]
    [ProducesResponseType(typeof(SessaoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar([FromBody] CriarSessaoDto dto)
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized();
        }

        var resultado = await _sessaoServico.CriarAsync(usuarioId, dto);

        if (!resultado.Sucesso)
        {
            return resultado.Erro switch
            {
                TipoErro.NaoEncontrado => Problem(statusCode: 404, detail: resultado.MensagemErro),
                TipoErro.Conflito => Problem(statusCode: 409, detail: resultado.MensagemErro),
                _ => Problem(statusCode: 400, detail: resultado.MensagemErro)
            };
        }

        return Created("", resultado.Valor);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SessaoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ListarMinhasSessoes()
    {
        var usuarioIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
        {
            return Unauthorized();
        }

        var eAdmin = User.IsInRole("admin");
        var resultado = await _sessaoServico.ListarMinhasSessoesAsync(usuarioId, eAdmin);

        return Ok(resultado);
    }
}