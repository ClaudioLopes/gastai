using GastAi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GastAi.Infrastructure.Persistence.Configurations;

public class LancamentoConfiguration : IEntityTypeConfiguration<Lancamento>
{
    public void Configure(EntityTypeBuilder<Lancamento> builder)
    {
        builder.ToTable("Lancamentos");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.Valor)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(l => l.Descricao)
            .HasMaxLength(200);

        builder.Property(l => l.Observacao)
            .HasMaxLength(500);

        builder.HasOne(l => l.Categoria)
            .WithMany()
            .HasForeignKey(l => l.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(l => l.Data);
        builder.HasIndex(l => l.GrupoParcelamentoId);
    }
}
