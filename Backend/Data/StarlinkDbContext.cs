using Microsoft.EntityFrameworkCore;
using Starlink.Api.Models;

namespace Starlink.Api.Data;

public class StarlinkDbContext(DbContextOptions<StarlinkDbContext> options) : DbContext(options)
{
    public DbSet<Dono> Donos => Set<Dono>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venda> Vendas => Set<Venda>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Dono>()
            .HasIndex(d => d.Email)
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasIndex(c => new { c.DonoId, c.Documento })
            .IsUnique();

        modelBuilder.Entity<Cliente>()
            .HasOne(c => c.Dono)
            .WithMany(d => d.Clientes)
            .HasForeignKey(c => c.DonoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venda>()
            .HasOne(v => v.Cliente)
            .WithMany(c => c.Vendas)
            .HasForeignKey(v => v.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Venda>()
            .HasOne(v => v.Dono)
            .WithMany(d => d.Vendas)
            .HasForeignKey(v => v.DonoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
