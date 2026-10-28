using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Servicos;

namespace Tatuagem.Api.Controllers;

[ApiController]
[Route("api/tatuadores")]
public class TatuadorController : ControllerBase
{
    private readonly TatuadorServico _tatuadorServico;

    public TatuadorController(TatuadorServico tatuadorServico)
    {
        _tatuadorServico = tatuadorServico;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PaginaResultadoDto<TatuadorResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? busca, [FromQuery] int pagina = 1, [FromQuery] int tamanho = 10)
    {
        var resultado = await _tatuadorServico.ListarPaginadoAsync(busca, pagina, tamanho);
        return Ok(resultado);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TatuadorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(int id)
    {
        var resultado = await _tatuadorServico.ObterPorIdAsync(id);
        if (!resultado.Sucesso)
        {
            return Problem(statusCode: 404, detail: resultado.MensagemErro);
        }

        return Ok(resultado.Valor);
    }

    [HttpPost]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(TatuadorResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Criar([FromBody] CriarTatuadorDto dto)
    {
        var resultado = await _tatuadorServico.CriarAsync(dto);
        if (!resultado.Sucesso)
        {
            return Problem(statusCode: 400, detail: resultado.MensagemErro);
        }

        return CreatedAtAction(nameof(ObterPorId), new { id = resultado.Valor!.Id }, resultado.Valor);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(typeof(TatuadorResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(int id, [FromBody] AtualizarTatuadorDto dto)
    {
        var resultado = await _tatuadorServico.AtualizarAsync(id, dto);
        if (!resultado.Sucesso)
        {
            return resultado.Erro == TipoErro.NaoEncontrado
                ? Problem(statusCode: 404, detail: resultado.MensagemErro)
                : Problem(statusCode: 400, detail: resultado.MensagemErro);
        }

        return Ok(resultado.Valor);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(int id)
    {
        var resultado = await _tatuadorServico.ExcluirAsync(id);
        if (!resultado.Sucesso)
        {
            return Problem(statusCode: 404, detail: resultado.MensagemErro);
        }

        return NoContent();
    }
}