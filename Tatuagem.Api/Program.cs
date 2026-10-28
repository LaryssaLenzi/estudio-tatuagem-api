using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Modelos;
using Tatuagem.Api.Servicos;

var builder = WebApplication.CreateBuilder(args);

// Configuração de Connection String
var connectionString = builder.Configuration.GetConnectionString("Padrao");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("A variável de ambiente ConnectionStrings__Padrao é obrigatória.");
}

// Entity Framework Core com PostgreSQL (Npgsql)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// Injeção de Dependência dos Serviços
builder.Services.AddScoped<AuthServico>();
builder.Services.AddScoped<TatuadorServico>();

// Health Checks
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

// Controllers e ProblemDetails
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

// Autenticação JWT
var jwtSegredo = builder.Configuration["Jwt:Segredo"];
if (string.IsNullOrWhiteSpace(jwtSegredo) || jwtSegredo.Length < 32)
{
    jwtSegredo = "chave-secreta-padrao-com-mais-de-32-caracteres-para-dev";
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSegredo)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// CORS
var origens = builder.Configuration["Cors:Origens"] ?? "http://localhost:5173";
var listaOrigens = origens.Split(',', StringSplitOptions.RemoveEmptyEntries);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(listaOrigens)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .WithExposedHeaders("Location");
    });
});

// Swagger (Swashbuckle 10 / OpenAPI .NET 2.x)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Estúdio de Tatuagem API",
        Version = "v1",
        Description = "Projeto escolar - empresa fictícia"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Insira o token JWT abaixo:"
    });

    c.AddSecurityRequirement(doc => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", doc), new List<string>() }
    });
});

var app = builder.Build();

// Endpoints de Saúde e Versão
app.MapHealthChecks("/health");
app.MapGet("/api/versao", () => Results.Ok(new
{
    commit = Environment.GetEnvironmentVariable("RENDER_GIT_COMMIT") ?? "local",
    ambiente = app.Environment.EnvironmentName
}));

// Swagger visível em produção
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tatuagem API v1"));

app.UseExceptionHandler();
app.UseStatusCodePages();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Autoredirecionamento da raiz para o Swagger
app.MapGet("/", () => Results.Redirect("/swagger"));

// Aplica Migrations e executa Seed ao subir
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed Idempotente de Usuários
    var adminSenha = builder.Configuration["Seed:AdminSenha"] ?? "Admin@2026";
    if (!await db.Usuarios.AnyAsync(u => u.Email == "admin@estudio.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = "Administrador",
            Email = "admin@estudio.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(adminSenha),
            Papel = "admin"
        });
    }

    if (!await db.Usuarios.AnyAsync(u => u.Email == "cliente1@exemplo.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = "Cliente Um",
            Email = "cliente1@exemplo.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
            Papel = "cliente"
        });
    }

    if (!await db.Usuarios.AnyAsync(u => u.Email == "cliente2@exemplo.com"))
    {
        db.Usuarios.Add(new Usuario
        {
            Nome = "Cliente Dois",
            Email = "cliente2@exemplo.com",
            SenhaHash = BCrypt.Net.BCrypt.HashPassword("Cliente123!"),
            Papel = "cliente"
        });
    }

    // Seed Idempotente de Tatuadores Realistas (8+)
    if (!await db.Tatuadores.AnyAsync())
    {
        db.Tatuadores.AddRange(
            new Tatuador { Nome = "Lucas Andrade", EstiloPrincipal = "Realismo Black & Grey", PrecoMinimo = 350.00m },
            new Tatuador { Nome = "Camila Torres", EstiloPrincipal = "Aquarela & Fineline", PrecoMinimo = 250.00m },
            new Tatuador { Nome = "Diego 'Caveira' Martins", EstiloPrincipal = "Old School / Tradicional", PrecoMinimo = 300.00m },
            new Tatuador { Nome = "Juliana Neves", EstiloPrincipal = "Botanico & Geométrico", PrecoMinimo = 280.00m },
            new Tatuador { Nome = "Rafael 'Kuro' Silva", EstiloPrincipal = "Irezumi / Oriental", PrecoMinimo = 400.00m },
            new Tatuador { Nome = "Mariana Costa", EstiloPrincipal = "Microrealismo & Pontilhismo", PrecoMinimo = 320.00m },
            new Tatuador { Nome = "Bruno 'Viking' Santos", EstiloPrincipal = "Neotradicional & Tribal", PrecoMinimo = 300.00m },
            new Tatuador { Nome = "Beatriz Lima", EstiloPrincipal = "Lettering & Chicano", PrecoMinimo = 220.00m }
        );
    }

    await db.SaveChangesAsync();
}

app.Run();

public partial class Program { }