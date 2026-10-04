using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Data;

public class LocadoraDbContext(DbContextOptions<LocadoraDbContext> options) : DbContext(options)
{
    public DbSet<Fabricante> Fabricantes => Set<Fabricante>();
    public DbSet<CategoriaVeiculo> CategoriasVeiculo => Set<CategoriaVeiculo>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Aluguel> Alugueis => Set<Aluguel>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LocadoraDbContext).Assembly);
    }
}
