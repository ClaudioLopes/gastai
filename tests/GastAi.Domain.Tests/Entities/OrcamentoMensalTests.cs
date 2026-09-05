using GastAi.Domain.Entities;
using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Tests.Entities;

public class OrcamentoMensalTests
{
    [Fact]
    public void Construtor_TruncaMesReferenciaParaPrimeiroDia()
    {
        var orcamento = new OrcamentoMensal(categoriaId: 1, new DateOnly(2026, 9, 17), valorLimite: 500m);

        Assert.Equal(new DateOnly(2026, 9, 1), orcamento.MesReferencia);
    }

    [Fact]
    public void Construtor_ValorLimiteNegativo_Lanca()
    {
        Assert.Throws<DomainException>(() => new OrcamentoMensal(1, new DateOnly(2026, 9, 1), -1m));
    }

    [Fact]
    public void Construtor_ValorLimiteZero_NaoLanca()
    {
        var exception = Record.Exception(() => new OrcamentoMensal(1, new DateOnly(2026, 9, 1), 0m));

        Assert.Null(exception);
    }

    [Fact]
    public void AtualizarLimite_ValorNegativo_Lanca()
    {
        var orcamento = new OrcamentoMensal(null, new DateOnly(2026, 9, 1), 500m);

        Assert.Throws<DomainException>(() => orcamento.AtualizarLimite(-1m));
    }

    [Fact]
    public void AtualizarLimite_ValorValido_Atualiza()
    {
        var orcamento = new OrcamentoMensal(null, new DateOnly(2026, 9, 1), 500m);

        orcamento.AtualizarLimite(800m);

        Assert.Equal(800m, orcamento.ValorLimite);
    }
}
