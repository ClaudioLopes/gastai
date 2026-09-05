using GastAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastAi.Infrastructure.Persistence.Configurations;

public class OrcamentoMensalConfiguration : IEntityTypeConfiguration<OrcamentoMensal>
{
    public void Configure(EntityTypeBuilder<OrcamentoMensal> builder)
    {
        builder.ToTable("OrcamentosMensais");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.ValorLimite)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.HasOne(o => o.Categoria)
            .WithMany()
            .HasForeignKey(o => o.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(o => new { o.CategoriaId, o.MesReferencia }).IsUnique();
    }
}
