using Microsoft.EntityFrameworkCore;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Modelos;

namespace Tatuagem.Api.Servicos;

public class TatuadorServico
{
    private readonly AppDbContext _db;
    private readonly ILogger<TatuadorServico> _logger;

    public TatuadorServico(AppDbContext db, ILogger<TatuadorServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<PaginaResultadoDto<TatuadorResponseDto>> ListarPaginadoAsync(string? busca, int pagina = 1, int tamanho = 10)
    {
        if (pagina < 1) pagina = 1;
        if (tamanho < 1) tamanho = 10;
        if (tamanho > 50) tamanho = 50;

        var query = _db.Tatuadores.AsQueryable();

        if (!string.IsNullOrWhiteSpace(busca))
        {
            var termo = busca.Trim().ToLower();
            query = query.Where(t => t.Nome.ToLower().Contains(termo) || t.EstiloPrincipal.ToLower().Contains(termo));
        }

        var total = await query.CountAsync();
        var totalPaginas = (int)Math.Ceiling(total / (double)tamanho);

        var itens = await query
            .OrderBy(t => t.Nome)
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .Select(t => new TatuadorResponseDto(t.Id, t.Nome, t.EstiloPrincipal, t.PrecoMinimo))
            .ToListAsync();

        return new PaginaResultadoDto<TatuadorResponseDto>(itens, pagina, tamanho, total, totalPaginas);
    }

    public async Task<Resultado<TatuadorResponseDto>> ObterPorIdAsync(int id)
    {
        var tatuador = await _db.Tatuadores.FindAsync(id);
        if (tatuador == null)
        {
            return Resultado<TatuadorResponseDto>.Falha(TipoErro.NaoEncontrado, "Tatuador não encontrado.");
        }

        return Resultado<TatuadorResponseDto>.Ok(new TatuadorResponseDto(tatuador.Id, tatuador.Nome, tatuador.EstiloPrincipal, tatuador.PrecoMinimo));
    }

    public async Task<Resultado<TatuadorResponseDto>> CriarAsync(CriarTatuadorDto dto)
    {
        var tatuador = new Tatuador
        {
            Nome = dto.Nome.Trim(),
            EstiloPrincipal = dto.EstiloPrincipal.Trim(),
            PrecoMinimo = dto.PrecoMinimo
        };

        _db.Tatuadores.Add(tatuador);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Tatuador criado com sucesso. ID: {Id}, Nome: {Nome}", tatuador.Id, tatuador.Nome);

        return Resultado<TatuadorResponseDto>.Ok(new TatuadorResponseDto(tatuador.Id, tatuador.Nome, tatuador.EstiloPrincipal, tatuador.PrecoMinimo));
    }

    public async Task<Resultado<TatuadorResponseDto>> AtualizarAsync(int id, AtualizarTatuadorDto dto)
    {
        var tatuador = await _db.Tatuadores.FindAsync(id);
        if (tatuador == null)
        {
            return Resultado<TatuadorResponseDto>.Falha(TipoErro.NaoEncontrado, "Tatuador não encontrado.");
        }

        tatuador.Nome = dto.Nome.Trim();
        tatuador.EstiloPrincipal = dto.EstiloPrincipal.Trim();
        tatuador.PrecoMinimo = dto.PrecoMinimo;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Tatuador atualizado. ID: {Id}", tatuador.Id);

        return Resultado<TatuadorResponseDto>.Ok(new TatuadorResponseDto(tatuador.Id, tatuador.Nome, tatuador.EstiloPrincipal, tatuador.PrecoMinimo));
    }

    public async Task<Resultado<bool>> ExcluirAsync(int id)
    {
        var tatuador = await _db.Tatuadores.FindAsync(id);
        if (tatuador == null)
        {
            return Resultado<bool>.Falha(TipoErro.NaoEncontrado, "Tatuador não encontrado.");
        }

        _db.Tatuadores.Remove(tatuador);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Tatuador excluído. ID: {Id}", id);

        return Resultado<bool>.Ok(true);
    }
}