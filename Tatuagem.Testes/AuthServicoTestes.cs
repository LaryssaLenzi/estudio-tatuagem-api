using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Servicos;
using Xunit;

namespace Tatuagem.Testes;

public class AuthServicoTestes : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _contextOptions;
    private readonly IConfiguration _config;

    public AuthServicoTestes()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_contextOptions);
        context.Database.EnsureCreated();

        var myConfig = new Dictionary<string, string?>
        {
            {"Jwt:Segredo", "chave-secreta-muito-longa-e-segura-para-testes-unitarios-12345"}
        };
        _config = new ConfigurationBuilder().AddInMemoryCollection(myConfig).Build();
    }

    public void Dispose() => _connection.Dispose();

    [Fact]
    public async Task RegistrarAsync_DeveCriarClienteComSucesso()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new AuthServico(context, _config, NullLogger<AuthServico>.Instance);
        var dto = new RegistroDto("Maria Souza", "maria@email.com", "Senha123!");

        // Act
        var resultado = await servico.RegistrarAsync(dto);

        // Assert
        Assert.True(resultado.Sucesso);
        Assert.NotNull(resultado.Valor);
        Assert.Equal("cliente", resultado.Valor.Papel);
        Assert.Equal("maria@email.com", resultado.Valor.Email);
    }

    [Fact]
    public async Task RegistrarAsync_DeveRetornarConflito_QuandoEmailJaExistir()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new AuthServico(context, _config, NullLogger<AuthServico>.Instance);
        var dto = new RegistroDto("Maria Souza", "duplicado@email.com", "Senha123!");

        await servico.RegistrarAsync(dto);

        // Act
        var resultadoDuplicado = await servico.RegistrarAsync(dto);

        // Assert
        Assert.False(resultadoDuplicado.Sucesso);
        Assert.Equal(TipoErro.Conflito, resultadoDuplicado.Erro);
    }

    [Fact]
    public async Task LoginAsync_DeveGerarTokenJwt_QuandoCredenciaisForemValidas()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new AuthServico(context, _config, NullLogger<AuthServico>.Instance);
        await servico.RegistrarAsync(new RegistroDto("Pedro", "pedro@email.com", "Senha123!"));

        // Act
        var resultadoLogin = await servico.LoginAsync(new LoginDto("pedro@email.com", "Senha123!"));

        // Assert
        Assert.True(resultadoLogin.Sucesso);
        Assert.NotNull(resultadoLogin.Valor);
        Assert.False(string.IsNullOrWhiteSpace(resultadoLogin.Valor.Token));
    }

    [Fact]
    public async Task LoginAsync_DeveFalhar_QuandoSenhaForIncorreta()
    {
        // Arrange
        using var context = new AppDbContext(_contextOptions);
        var servico = new AuthServico(context, _config, NullLogger<AuthServico>.Instance);
        await servico.RegistrarAsync(new RegistroDto("Pedro", "pedro2@email.com", "Senha123!"));

        // Act
        var resultadoLogin = await servico.LoginAsync(new LoginDto("pedro2@email.com", "SenhaErrada"));

        // Assert
        Assert.False(resultadoLogin.Sucesso);
        Assert.Equal(TipoErro.Validacao, resultadoLogin.Erro);
    }
}