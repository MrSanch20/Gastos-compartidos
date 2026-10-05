using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosCompartidos.Api.Data.Configuraciones;

public class ParticipacionGastoConfiguration : IEntityTypeConfiguration<ParticipacionGasto>
{
    public void Configure(EntityTypeBuilder<ParticipacionGasto> builder)
    {
        builder.HasKey(p => new { p.GastoId, p.UsuarioId });

        builder.Property(p => p.Importe)
            .HasPrecision(18, 2);

        builder.HasOne(p => p.Gasto)
            .WithMany(g => g.Participaciones)
            .HasForeignKey(p => p.GastoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Usuario)
            .WithMany(u => u.Participaciones)
            .HasForeignKey(p => p.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
