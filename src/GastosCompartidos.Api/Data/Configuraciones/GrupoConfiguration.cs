using GastosCompartidos.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastosCompartidos.Api.Data.Configuraciones;

public class GrupoConfiguration : IEntityTypeConfiguration<Grupo>
{
    public void Configure(EntityTypeBuilder<Grupo> builder)
    {
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Nombre)
            .IsRequired()
            .HasMaxLength(100);
    }
}
