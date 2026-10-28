using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using Tatuagem.Api.Dados;

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
}

app.Run();

public partial class Program { }