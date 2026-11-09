using GastAi.Application.Abstractions;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Application.Lancamentos;

public class CompraAutomaticaService(
    ILancamentoRepository lancamentos,
    ICategoriaRepository categorias,
    IUnitOfWork unitOfWork,
    TimeProvider relogio) : ICompraAutomaticaService
{
    private const string NomeCategoriaPadrao = "Outros";
    private const string DescricaoSemEstabelecimento = "Compra no cartão";

    /// <summary>O Brasil não tem horário de verão desde 2019, então Brasília é sempre UTC-3.</summary>
    private static readonly TimeSpan FusoBrasilia = TimeSpan.FromHours(-3);

    public async Task<LancamentoDto> RegistrarAsync(CompraAutomaticaRequest request, CancellationToken ct = default)
    {
        var descricao = string.IsNullOrWhiteSpace(request.Estabelecimento)
            ? DescricaoSemEstabelecimento
            : request.Estabelecimento.Trim();

        var categoria = await EscolherCategoriaAsync(descricao, ct);
        var data = request.Data ?? DateOnly.FromDateTime(relogio.GetUtcNow().ToOffset(FusoBrasilia).DateTime);
        var formaPagamento = request.FormaPagamento ?? FormaPagamento.Credito;

        var observacao = string.IsNullOrWhiteSpace(request.Cartao)
            ? "Registrada automaticamente"
            : $"Registrada automaticamente ({request.Cartao.Trim()})";

        var lancamento = Lancamento.NovoGastoAutomatico(
            request.Valor, data, descricao, categoria.Id, formaPagamento, observacao);

        await lancamentos.AddAsync(lancamento, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new LancamentoDto(
            lancamento.Id, lancamento.Tipo, lancamento.Valor, lancamento.Data, lancamento.Descricao,
            categoria.Id, categoria.Nome, lancamento.FormaPagamento, lancamento.Observacao,
            AConfirmar: lancamento.AConfirmar);
    }

    private async Task<Categoria> EscolherCategoriaAsync(string descricao, CancellationToken ct)
    {
        var ativas = await categorias.GetAllAsync(incluirInativas: false, ct);
        var gastos = ativas.Where(c => c.Tipo == TipoLancamento.Gasto).ToList();

        var idDoHistorico = await lancamentos.GetCategoriaDaUltimaCompraAsync(descricao, ct);
        var doHistorico = gastos.FirstOrDefault(c => c.Id == idDoHistorico);

        return doHistorico
            ?? gastos.FirstOrDefault(c => string.Equals(c.Nome, NomeCategoriaPadrao, StringComparison.OrdinalIgnoreCase))
            ?? gastos.FirstOrDefault()
            ?? throw new DomainException("Não há categoria de gasto ativa para registrar a compra.");
    }
}
