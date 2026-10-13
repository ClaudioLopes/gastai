using System.Globalization;
using System.Text;
using GastAi.Application.Abstractions;
using GastAi.Contracts.Relatorios;
using GastAi.Domain.Enums;

namespace GastAi.Application.Relatorios;

public class GastosRecorrentesService(ILancamentoRepository lancamentos) : IGastosRecorrentesService
{
    private const int MinMeses = 2;
    private const int MaxMeses = 12;
    private const int MinMesesComGasto = 2;

    /// <summary>Variações dentro dessa faixa (em %) são tratadas como "estável".</summary>
    private const decimal LimiteEstabilidadePercentual = 2m;

    public async Task<IReadOnlyList<GastoRecorrenteDto>> ListarAsync(
        int ano, int mes, int meses = 6, CancellationToken ct = default)
    {
        meses = Math.Clamp(meses, MinMeses, MaxMeses);

        var mesAtual = new DateOnly(ano, mes, 1);
        var primeiroMes = mesAtual.AddMonths(-(meses - 1));
        var janela = Enumerable.Range(0, meses).Select(i => primeiroMes.AddMonths(i)).ToList();

        var doPeriodo = await lancamentos.GetByPeriodoAsync(primeiroMes, mesAtual.AddMonths(1), ct);

        // Parcelas de compras parceladas se repetem por natureza e não são contas fixas.
        var candidatos = doPeriodo
            .Where(l => l.Tipo == TipoLancamento.Gasto
                        && l.GrupoParcelamentoId is null
                        && !string.IsNullOrWhiteSpace(l.Descricao))
            .GroupBy(l => (l.CategoriaId, Chave: Normalizar(l.Descricao)));

        var resultado = new List<GastoRecorrenteDto>();

        foreach (var grupo in candidatos)
        {
            var porMes = grupo
                .GroupBy(l => new DateOnly(l.Data.Year, l.Data.Month, 1))
                .ToDictionary(g => g.Key, g => g.Sum(l => l.Valor));

            if (porMes.Count < MinMesesComGasto)
                continue;

            var serie = janela
                .Select(m => new ValorMensalDto(m.Year, m.Month, porMes.GetValueOrDefault(m)))
                .ToList();

            var valorAtual = porMes.TryGetValue(mesAtual, out var atual) ? atual : (decimal?)null;
            var valorAnterior = porMes
                .Where(p => p.Key < mesAtual)
                .OrderByDescending(p => p.Key)
                .Select(p => (decimal?)p.Value)
                .FirstOrDefault();

            var variacao = valorAtual is not null && valorAnterior is > 0
                ? Math.Round((valorAtual.Value - valorAnterior.Value) / valorAnterior.Value * 100, 2)
                : (decimal?)null;

            var maisRecente = grupo.OrderByDescending(l => l.Data).First();

            resultado.Add(new GastoRecorrenteDto(
                maisRecente.CategoriaId,
                maisRecente.Categoria!.Nome,
                maisRecente.Descricao,
                serie,
                valorAtual,
                valorAnterior,
                variacao,
                ClassificarTendencia(variacao)));
        }

        // Os que já aparecem no mês selecionado primeiro; dentro de cada bloco, os mais caros no topo.
        return resultado
            .OrderByDescending(r => r.ValorAtual is not null)
            .ThenByDescending(r => r.ValorAtual ?? r.ValorAnterior ?? 0)
            .ToList();
    }

    private static TendenciaGasto ClassificarTendencia(decimal? variacao) => variacao switch
    {
        null => TendenciaGasto.Estavel,
        > LimiteEstabilidadePercentual => TendenciaGasto.Subindo,
        < -LimiteEstabilidadePercentual => TendenciaGasto.Caindo,
        _ => TendenciaGasto.Estavel
    };

    /// <summary>Ignora caixa, acentos e espaços repetidos: "Conta de  Luz" e "conta de luz" são o mesmo gasto.</summary>
    private static string Normalizar(string descricao)
    {
        var semAcento = new StringBuilder();
        foreach (var c in descricao.Trim().Normalize(NormalizationForm.FormD))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                semAcento.Append(c);
        }

        return string.Join(' ', semAcento.ToString().ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
