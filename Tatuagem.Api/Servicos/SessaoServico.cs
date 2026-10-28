using Microsoft.EntityFrameworkCore;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Modelos;

namespace Tatuagem.Api.Servicos;

public class SessaoServico
{
    private readonly AppDbContext _db;
    private readonly ILogger<SessaoServico> _logger;

    public SessaoServico(AppDbContext db, ILogger<SessaoServico> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Resultado<SessaoResponseDto>> CriarAsync(int usuarioId, CriarSessaoDto dto)
    {
        var dataHoraUtc = dto.DataHora.UtcDateTime;

        // Inicia transação para garantir isolamento na verificação de conflito de horário
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var tatuadorExiste = await _db.Tatuadores.AnyAsync(t => t.Id == dto.TatuadorId);
            if (!tatuadorExiste)
            {
                return Resultado<SessaoResponseDto>.Falha(TipoErro.NaoEncontrado, "Tatuador não encontrado.");
            }

            // Regra do 409 Conflict: Tatuador já possui uma sessão agendada naquele mesmo horário
            var conflito = await _db.Sessoes.AnyAsync(s => s.TatuadorId == dto.TatuadorId && s.DataHoraUtc == dataHoraUtc);
            if (conflito)
            {
                _logger.LogWarning("Conflito de agendamento: Tatuador ID {TatuadorId} já possui sessão às {DataHora}", dto.TatuadorId, dataHoraUtc);
                return Resultado<SessaoResponseDto>.Falha(TipoErro.Conflito, "Este tatuador já possui uma sessão agendada neste mesmo horário.");
            }

            var sessao = new Sessao
            {
                TatuadorId = dto.TatuadorId,
                UsuarioId = usuarioId,
                DataHoraUtc = dataHoraUtc,
                DescricaoTatuagem = dto.DescricaoTatuagem.Trim()
            };

            _db.Sessoes.Add(sessao);
            await _db.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Sessão criada com sucesso. ID: {Id}, TatuadorID: {TatuadorId}, UsuarioID: {UsuarioId}", sessao.Id, sessao.TatuadorId, sessao.UsuarioId);

            var tatuador = await _db.Tatuadores.FindAsync(dto.TatuadorId);
            var usuario = await _db.Usuarios.FindAsync(usuarioId);

            return Resultado<SessaoResponseDto>.Ok(new SessaoResponseDto(
                sessao.Id,
                sessao.TatuadorId,
                tatuador?.Nome ?? string.Empty,
                sessao.UsuarioId,
                usuario?.Nome ?? string.Empty,
                sessao.DataHoraUtc,
                sessao.DescricaoTatuagem
            ));
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Erro ao criar sessão de agendamento.");
            throw;
        }
    }

    public async Task<List<SessaoResponseDto>> ListarMinhasSessoesAsync(int usuarioId, bool eAdmin)
    {
        var query = _db.Sessoes
            .Include(s => s.Tatuador)
            .Include(s => s.Usuario)
            .AsQueryable();

        // Se for cliente comum, filtra apenas as próprias sessões; admin vê todas
        if (!eAdmin)
        {
            query = query.Where(s => s.UsuarioId == usuarioId);
        }

        return await query
            .OrderBy(s => s.DataHoraUtc)
            .Select(s => new SessaoResponseDto(
                s.Id,
                s.TatuadorId,
                s.Tatuador != null ? s.Tatuador.Nome : string.Empty,
                s.UsuarioId,
                s.Usuario != null ? s.Usuario.Nome : string.Empty,
                s.DataHoraUtc,
                s.DescricaoTatuagem
            ))
            .ToListAsync();
    }
}