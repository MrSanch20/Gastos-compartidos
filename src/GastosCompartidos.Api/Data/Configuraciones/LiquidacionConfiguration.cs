using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosCompartidos.Api.Data.Configuraciones;

public class LiquidacionConfiguration : IEntityTypeConfiguration<Liquidacion>
{
    public void Configure(EntityTypeBuilder<Liquidacion> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Importe)
            .HasPrecision(18, 2);

        builder.HasOne(l => l.Grupo)
            .WithMany(g => g.Liquidaciones)
            .HasForeignKey(l => l.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.De)
            .WithMany()
            .HasForeignKey(l => l.DeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Para)
            .WithMany()
            .HasForeignKey(l => l.ParaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
