using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocadoraVeiculos.Api.Data.Configurations;

public class FabricanteConfiguration : IEntityTypeConfiguration<Fabricante>
{
    public void Configure(EntityTypeBuilder<Fabricante> builder)
    {
        builder.ToTable("Fabricantes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PaisOrigem).HasMaxLength(80);
        builder.HasIndex(x => x.Nome).IsUnique();
    }
}
