using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosCompartidos.Api.Data.Configuraciones;

public class MiembroGrupoConfiguration : IEntityTypeConfiguration<MiembroGrupo>
{
    public void Configure(EntityTypeBuilder<MiembroGrupo> builder)
    {
        builder.HasKey(m => new { m.GrupoId, m.UsuarioId });

        builder.HasOne(m => m.Grupo)
            .WithMany(g => g.Miembros)
            .HasForeignKey(m => m.GrupoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(m => m.Usuario)
            .WithMany(u => u.GruposPertenecientes)
            .HasForeignKey(m => m.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
