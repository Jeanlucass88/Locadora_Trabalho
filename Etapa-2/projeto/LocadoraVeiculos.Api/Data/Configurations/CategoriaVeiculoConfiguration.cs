using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocadoraVeiculos.Api.Data.Configurations;

public class CategoriaVeiculoConfiguration : IEntityTypeConfiguration<CategoriaVeiculo>
{
    public void Configure(EntityTypeBuilder<CategoriaVeiculo> builder)
    {
        builder.ToTable("CategoriasVeiculo");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descricao).HasMaxLength(250);
        builder.HasIndex(x => x.Nome).IsUnique();
    }
}
