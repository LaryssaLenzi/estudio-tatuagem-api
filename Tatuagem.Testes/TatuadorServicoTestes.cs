using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Servicos;
using Xunit;

namespace Tatuagem.Testes;

public class TatuadorServicoTestes : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;

    public TatuadorServicoTestes()
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
    public async Task CriarAsync_DeveAdicionarTatuadorComSucesso()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new TatuadorServico(context, NullLogger<TatuadorServico>.Instance);
        var dto = new CriarTatuadorDto("Diego Martins", "Tradicional", 300);

        // Act
        var resultado = await servico.CriarAsync(dto);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("Diego Martins", resultado.Valor.Nome);
    }

    [Fact]
    public async Task ListarPaginadoAsync_DeveFiltrarPorBusca_IgnorandoMaiusculasEMinusculas()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new TatuadorServico(context, NullLogger<TatuadorServico>.Instance);

        await servico.CriarAsync(new CriarTatuadorDto("Juliana Neves", "Geométrico", 280));
        await servico.CriarAsync(new CriarTatuadorDto("Rafael Silva", "Oriental", 400));

        // Act
        var resultado = await servico.ListarPaginadoAsync("juliana", pagina: 1, tamanho: 10);

        // Assert
        Assert.Single(resultado.Itens);
        Assert.Equal("Juliana Neves", resultado.Itens[0].Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarNaoEncontrado_QuandoIdNaoExistir()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new TatuadorServico(context, NullLogger<TatuadorServico>.Instance);

        // Act
        var resultado = await servico.ObterPorIdAsync(999);

        // Assert
        Assert.False(resultado.Sucesso);
        Assert.Equal(TipoErro.NaoEncontrado, resultado.Erro);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRemoverTatuadorComSucesso()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new TatuadorServico(context, NullLogger<TatuadorServico>.Instance);
        var criado = await servico.CriarAsync(new CriarTatuadorDto("Mariana Costa", "Pontilhismo", 320));

        // Act
        var resultadoExclusao = await servico.ExcluirAsync(criado.Valor!.Id);

        // Assert
        Assert.True(resultadoExclusao.Sucesso);
        var buscarRemovido = await servico.ObterPorIdAsync(criado.Valor.Id);
        Assert.False(buscarRemovido.Sucesso);
    }
}