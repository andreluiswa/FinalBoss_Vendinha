using Microsoft.EntityFrameworkCore;
using FinalBoss_Vendinha.Models;

namespace FinalBoss_Vendinha.Data;

public class VendinhaDbContext : DbContext
{
    private readonly string _connectionString;

    public VendinhaDbContext(string connectionString)
    {
        _connectionString = connectionString;
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<Divida> Dividas => Set<Divida>();
    public DbSet<HistoricoJuros> HistoricoJuros => Set<HistoricoJuros>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(_connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasIndex(cliente => cliente.Cpf)
            .IsUnique();

        modelBuilder.Entity<Divida>()
            .HasIndex(divida => divida.ClienteId)
            .IsUnique()
            .HasFilter("Paga = 0");

        modelBuilder.Entity<Venda>()
            .HasOne(venda => venda.Cliente)
            .WithMany(cliente => cliente.Vendas)
            .HasForeignKey(venda => venda.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Venda>()
            .HasOne(venda => venda.Divida)
            .WithMany(divida => divida.Vendas)
            .HasForeignKey(venda => venda.DividaId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Divida>()
            .HasOne(divida => divida.Cliente)
            .WithMany(cliente => cliente.Dividas)
            .HasForeignKey(divida => divida.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
