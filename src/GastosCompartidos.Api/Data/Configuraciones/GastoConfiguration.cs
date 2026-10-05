using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosCompartidos.Api.Data.Configuraciones;

public class GastoConfiguration : IEntityTypeConfiguration<Gasto>
{
    public void Configure(EntityTypeBuilder<Gasto> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Concepto)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(g => g.Importe)
            .HasPrecision(18, 2);

        builder.HasOne(g => g.Grupo)
            .WithMany(gr => gr.Gastos)
            .HasForeignKey(g => g.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(g => g.PagadoPor)
            .WithMany(u => u.GastosPagados)
            .HasForeignKey(g => g.PagadoPorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
