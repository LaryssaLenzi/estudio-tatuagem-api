using Microsoft.EntityFrameworkCore;
using Tatuagem.Api.Modelos;

namespace Tatuagem.Api.Dados;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tatuador> Tatuadores => Set<Tatuador>();
    public DbSet<Sessao> Sessoes => Set<Sessao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
        });

        modelBuilder.Entity<Tatuador>(e =>
        {
            e.HasKey(t => t.Id);
        });

        modelBuilder.Entity<Sessao>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasOne(s => s.Tatuador)
             .WithMany(t => t.Sessoes)
             .HasForeignKey(s => s.TatuadorId);

            e.HasOne(s => s.Usuario)
             .WithMany()
             .HasForeignKey(s => s.UsuarioId);
        });
    }
}