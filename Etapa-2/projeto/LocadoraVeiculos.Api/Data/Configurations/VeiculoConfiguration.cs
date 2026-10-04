using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocadoraVeiculos.Api.Data.Configurations;

public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("Veiculos", table =>
        {
            table.HasCheckConstraint("CK_Veiculos_AnoFabricacao", "[AnoFabricacao] BETWEEN 1886 AND 2100");
            table.HasCheckConstraint("CK_Veiculos_Quilometragem", "[Quilometragem] >= 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Modelo).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Placa).HasMaxLength(8).IsUnicode(false).IsRequired();
        builder.Property(x => x.Chassi).HasMaxLength(17).IsUnicode(false).IsRequired();
        builder.Property(x => x.Ativo).HasDefaultValue(true);

        builder.HasIndex(x => x.Placa).IsUnique();
        builder.HasIndex(x => x.Chassi).IsUnique();

        builder.HasOne(x => x.Fabricante)
            .WithMany(x => x.Veiculos)
            .HasForeignKey(x => x.FabricanteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CategoriaVeiculo)
            .WithMany(x => x.Veiculos)
            .HasForeignKey(x => x.CategoriaVeiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
