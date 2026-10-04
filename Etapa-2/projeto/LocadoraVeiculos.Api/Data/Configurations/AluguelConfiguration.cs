using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocadoraVeiculos.Api.Data.Configurations;

public class AluguelConfiguration : IEntityTypeConfiguration<Aluguel>
{
    public void Configure(EntityTypeBuilder<Aluguel> builder)
    {
        builder.ToTable("Alugueis", table =>
        {
            table.HasCheckConstraint("CK_Alugueis_PeriodoPrevisto", "[DataPrevistaDevolucao] > [DataRetirada]");
            table.HasCheckConstraint("CK_Alugueis_DataDevolucao", "[DataDevolucao] IS NULL OR [DataDevolucao] >= [DataRetirada]");
            table.HasCheckConstraint("CK_Alugueis_QuilometragemInicial", "[QuilometragemInicial] >= 0");
            table.HasCheckConstraint("CK_Alugueis_QuilometragemFinal", "[QuilometragemFinal] IS NULL OR [QuilometragemFinal] >= [QuilometragemInicial]");
            table.HasCheckConstraint("CK_Alugueis_ValorDiaria", "[ValorDiaria] > 0");
            table.HasCheckConstraint("CK_Alugueis_ValorTotal", "[ValorTotal] IS NULL OR [ValorTotal] >= 0");
            table.HasCheckConstraint(
                "CK_Alugueis_DevolucaoCompleta",
                "([DataDevolucao] IS NULL AND [QuilometragemFinal] IS NULL AND [ValorTotal] IS NULL) OR " +
                "([DataDevolucao] IS NOT NULL AND [QuilometragemFinal] IS NOT NULL AND [ValorTotal] IS NOT NULL)");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.ValorDiaria).HasPrecision(10, 2);
        builder.Property(x => x.ValorTotal).HasPrecision(12, 2);
        builder.HasIndex(x => new { x.VeiculoId, x.DataRetirada, x.DataPrevistaDevolucao });
        builder.HasIndex(x => new { x.ClienteId, x.DataRetirada });

        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.Alugueis)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Veiculo)
            .WithMany(x => x.Alugueis)
            .HasForeignKey(x => x.VeiculoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
