using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Entities;

public class Lancamento
{
    public int Id { get; private set; }
    public TipoLancamento Tipo { get; private set; }
    public decimal Valor { get; private set; }
    public DateOnly Data { get; private set; }
    public string Descricao { get; private set; } = string.Empty;
    public int CategoriaId { get; private set; }
    public Categoria? Categoria { get; private set; }
    public FormaPagamento? FormaPagamento { get; private set; }
    public string? Observacao { get; private set; }

    private Lancamento()
    {
    }

    private Lancamento(
        TipoLancamento tipo,
        decimal valor,
        DateOnly data,
        string descricao,
        int categoriaId,
        FormaPagamento? formaPagamento,
        string? observacao)
    {
        if (valor <= 0)
            throw new DomainException("O valor do lançamento deve ser maior que zero.");

        if (tipo == TipoLancamento.Gasto && formaPagamento is null)
            throw new DomainException("A forma de pagamento é obrigatória para um gasto.");

        if (tipo == TipoLancamento.Receita && formaPagamento is not null)
            throw new DomainException("Forma de pagamento não se aplica a uma receita.");

        Tipo = tipo;
        Valor = valor;
        Data = data;
        Descricao = descricao?.Trim() ?? string.Empty;
        CategoriaId = categoriaId;
        FormaPagamento = formaPagamento;
        Observacao = observacao;
    }

    public static Lancamento NovoGasto(
        decimal valor,
        DateOnly data,
        string descricao,
        int categoriaId,
        FormaPagamento formaPagamento,
        string? observacao = null)
        => new(TipoLancamento.Gasto, valor, data, descricao, categoriaId, formaPagamento, observacao);

    public static Lancamento NovaReceita(
        decimal valor,
        DateOnly data,
        string descricao,
        int categoriaId,
        string? observacao = null)
        => new(TipoLancamento.Receita, valor, data, descricao, categoriaId, null, observacao);

    public void Atualizar(
        decimal valor,
        DateOnly data,
        string descricao,
        int categoriaId,
        FormaPagamento? formaPagamento,
        string? observacao)
    {
        if (valor <= 0)
            throw new DomainException("O valor do lançamento deve ser maior que zero.");

        if (Tipo == TipoLancamento.Gasto && formaPagamento is null)
            throw new DomainException("A forma de pagamento é obrigatória para um gasto.");

        if (Tipo == TipoLancamento.Receita && formaPagamento is not null)
            throw new DomainException("Forma de pagamento não se aplica a uma receita.");

        Valor = valor;
        Data = data;
        Descricao = descricao?.Trim() ?? string.Empty;
        CategoriaId = categoriaId;
        FormaPagamento = formaPagamento;
        Observacao = observacao;
    }
}
