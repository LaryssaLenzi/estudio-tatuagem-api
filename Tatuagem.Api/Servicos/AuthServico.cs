using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Tatuagem.Api.Dados;
using Tatuagem.Api.Dtos;
using Tatuagem.Api.Modelos;

namespace Tatuagem.Api.Servicos;

public class AuthServico
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly ILogger<AuthServico> _logger;

    public AuthServico(AppDbContext db, IConfiguration config, ILogger<AuthServico> logger)
    {
        _db = db;
        _config = config;
        _logger = logger;
    }

    public async Task<Resultado<UsuarioResponseDto>> RegistrarAsync(RegistroDto dto)
    {
        if (await _db.Usuarios.AnyAsync(u => u.Email == dto.Email.ToLower()))
        {
            _logger.LogWarning("Tentativa de registro com e-mail já existente: {Email}", dto.Email);
            return Resultado<UsuarioResponseDto>.Falha(TipoErro.Conflito, "Este e-mail já está cadastrado.");
        }

        var usuario = new Usuario
        {
            Nome = dto.Nome.Trim(),
            Email = dto.Email.ToLower().Trim(),
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(dto.Senha),
            Papel = "cliente"
        };

        _db.Usuarios.Add(usuario);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Usuário registrado com sucesso: {Email}", usuario.Email);

        return Resultado<UsuarioResponseDto>.Ok(new UsuarioResponseDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel));
    }

    public async Task<Resultado<LoginResponseDto>> LoginAsync(LoginDto dto)
    {
        var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Email == dto.Email.ToLower());

        if (usuario == null || !BCrypt.Net.BCrypt.Verify(dto.Senha, usuario.SenhaHash))
        {
            _logger.LogWarning("Falha no login para o e-mail: {Email}", dto.Email);
            return Resultado<LoginResponseDto>.Falha(TipoErro.Validacao, "E-mail ou senha inválidos.");
        }

        var token = GerarTokenJwt(usuario);
        _logger.LogInformation("Login bem-sucedido para: {Email}", usuario.Email);

        return Resultado<LoginResponseDto>.Ok(new LoginResponseDto(token, usuario.Nome, usuario.Email, usuario.Papel));
    }

    private string GerarTokenJwt(Usuario usuario)
    {
        var jwtSegredo = _config["Jwt:Segredo"] ?? "chave-secreta-padrao-com-mais-de-32-caracteres-para-dev";
        var key = Encoding.UTF8.GetBytes(jwtSegredo);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim(ClaimTypes.Name, usuario.Nome),
            new Claim(ClaimTypes.Email, usuario.Email),
            new Claim(ClaimTypes.Role, usuario.Papel)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}