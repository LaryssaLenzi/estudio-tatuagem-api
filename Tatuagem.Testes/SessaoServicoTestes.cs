using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Modelos;
using Tatuagem.Api.Servicos;
using Xunit;

namespace Tatuagem.Testes;

public class SessaoServicoTestes : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;

    public SessaoServicoTestes()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_contextOptions);
        context.Database.EnsureCreated();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task CriarAsync_DeveRetornar409Conflito_QuandoTatuadorJaPossuirSessaoNoMesmoHorario()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new SessaoServico(context, NullLogger<SessaoServico>.Instance);

        var tatuador = new Tatuador { Nome = "Lucas Andrade", EstiloPrincipal = "Realismo", PrecoMinimo = 300 };
        var usuario = new Usuario { Nome = "Cliente Teste", Email = "cliente@teste.com", SenhaHash = "hash" };
        context.Tatuadores.Add(tatuador);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var dataHora = DateTimeOffset.UtcNow.AddDays(1);
        var dto = new CriarSessaoDto(tatuador.Id, dataHora, "Tatuagem Leão no Braço");

        // Primeira sessão no horário
        await servico.CriarAsync(usuario.Id, dto);

        // Act - Tentativa de agendar a SEGUNDA sessão no MESMO horário
        var resultadoConflito = await servico.CriarAsync(usuario.Id, dto);

        // Assert
        Assert.False(resultadoConflito.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultadoConflito.Erro);
        Assert.Contains("já possui uma sessão agendada neste mesmo horário", resultadoConflito.MensagemErro);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarSessaoComSucesso_QuandoHorarioEstiverLivre()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new SessaoServico(context, NullLogger<SessaoServico>.Instance);

        var tatuador = new Tatuador { Nome = "Camila Torres", EstiloPrincipal = "Fineline", PrecoMinimo = 250 };
        var usuario = new Usuario { Nome = "Cliente Teste 2", Email = "cliente2@teste.com", SenhaHash = "hash" };
        context.Tatuadores.Add(tatuador);
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        var dto = new CriarSessaoDto(tatuador.Id, DateTimeOffset.UtcNow.AddDays(2), "Tatuagem Flor de Lótus");

        // Act
        var resultado = await servico.CriarAsync(usuario.Id, dto);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("Tatuagem Flor de Lótus", resultado.Valor.DescricaoTatuagem);
    }

    [Fact]
    public async Task ListarMinhasSessoesAsync_DeveFiltrarPorUsuario_QuandoNaoForAdmin()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new SessaoServico(context, NullLogger<SessaoServico>.Instance);

        var u1 = new Usuario { Nome = "User 1", Email = "u1@teste.com", SenhaHash = "h" };
        var u2 = new Usuario { Nome = "User 2", Email = "u2@teste.com", SenhaHash = "h" };
        var tat = new Tatuador { Nome = "Tatuador 1", EstiloPrincipal = "Old School", PrecoMinimo = 200 };
        context.AddRange(u1, u2, tat);
        await context.SaveChangesAsync();

        await servico.CriarAsync(u1.Id, new CriarSessaoDto(tat.Id, DateTimeOffset.UtcNow.AddDays(1), "Sessão User 1"));
        await servico.CriarAsync(u2.Id, new CriarSessaoDto(tat.Id, DateTimeOffset.UtcNow.AddDays(2), "Sessão User 2"));

        // Act
        var sessoesUser1 = await servico.ListarMinhasSessoesAsync(u1.Id, eAdmin: false);

        // Assert
        Assert.Single(sessoesUser1);
        Assert.Equal("Sessão User 1", sessoesUser1[0].DescricaoTatuagem);
    }
}