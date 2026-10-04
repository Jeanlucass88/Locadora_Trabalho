using LocadoraVeiculos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LocadoraVeiculos.Api.Data.Configurations;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("Clientes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Nome).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Cpf).HasMaxLength(11).IsUnicode(false).IsFixedLength().IsRequired();
        builder.Property(x => x.Email).HasMaxLength(254).IsRequired();
        builder.Property(x => x.Telefone).HasMaxLength(20).IsUnicode(false);
        builder.Property(x => x.DataNascimento).HasColumnType("date");
        builder.Property(x => x.NumeroCnh).HasMaxLength(11).IsUnicode(false).IsRequired();
        builder.Property(x => x.ValidadeCnh).HasColumnType("date");
        builder.Property(x => x.Ativo).HasDefaultValue(true);

        builder.HasIndex(x => x.Cpf).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasIndex(x => x.NumeroCnh).IsUnique();
    }
}
