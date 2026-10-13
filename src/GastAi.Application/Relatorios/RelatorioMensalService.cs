using GastAi.Application.Abstractions;
using GastAi.Contracts.Relatorios;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;

namespace GastAi.Application.Relatorios;

public class RelatorioMensalService(ILancamentoRepository lancamentos) : IRelatorioMensalService
{
    public async Task<RelatorioMensalDto> GerarAsync(int ano, int mes, CancellationToken ct = default)
    {
        var doMes = await lancamentos.GetByMesAsync(ano, mes, ct);

        var mesAnterior = new DateOnly(ano, mes, 1).AddMonths(-1);
        var doMesAnterior = await lancamentos.GetByMesAsync(mesAnterior.Year, mesAnterior.Month, ct);

        var totalReceitas = Somar(doMes, TipoLancamento.Receita);
        var totalGastos = Somar(doMes, TipoLancamento.Gasto);
        var saldo = totalReceitas - totalGastos;

        var gastosPorCategoria = GastosPorCategoria(doMes, doMesAnterior, totalGastos);

        var totalReceitasMesAnterior = Somar(doMesAnterior, TipoLancamento.Receita);
        var totalGastosMesAnterior = Somar(doMesAnterior, TipoLancamento.Gasto);

        var comparacao = new ComparacaoMesAnteriorDto(
            ExisteDadosMesAnterior: doMesAnterior.Count > 0,
            TotalGastosMesAnterior: totalGastosMesAnterior,
            VariacaoPercentualGastos: VariacaoPercentual(totalGastosMesAnterior, totalGastos),
            TotalReceitasMesAnterior: totalReceitasMesAnterior,
            VariacaoPercentualReceitas: VariacaoPercentual(totalReceitasMesAnterior, totalReceitas),
            SaldoMesAnterior: totalReceitasMesAnterior - totalGastosMesAnterior);

        return new RelatorioMensalDto(ano, mes, totalReceitas, totalGastos, saldo, gastosPorCategoria, comparacao);
    }

    /// <summary>
    /// Inclui também as categorias que tiveram gasto só no mês anterior (Valor 0 no mês atual),
    /// para que quem parou de gastar com algo apareça na comparação.
    /// </summary>
    private static List<GastoPorCategoriaDto> GastosPorCategoria(
        List<Lancamento> doMes, List<Lancamento> doMesAnterior, decimal totalGastos)
    {
        var atuais = doMes
            .Where(l => l.Tipo == TipoLancamento.Gasto)
            .GroupBy(l => l.Categoria!)
            .ToDictionary(g => g.Key.Id, g => (Categoria: g.Key, Valor: g.Sum(l => l.Valor)));

        var anteriores = doMesAnterior
            .Where(l => l.Tipo == TipoLancamento.Gasto)
            .GroupBy(l => l.Categoria!)
            .ToDictionary(g => g.Key.Id, g => (Categoria: g.Key, Valor: g.Sum(l => l.Valor)));

        return atuais.Keys.Union(anteriores.Keys)
            .Select(id =>
            {
                var categoria = atuais.TryGetValue(id, out var a) ? a.Categoria : anteriores[id].Categoria;
                var valor = a.Valor;
                var valorAnterior = anteriores.TryGetValue(id, out var p) ? p.Valor : 0m;

                return new GastoPorCategoriaDto(
                    id,
                    categoria.Nome,
                    valor,
                    totalGastos == 0 ? 0 : Math.Round(valor / totalGastos * 100, 2),
                    valorAnterior,
                    VariacaoPercentual(valorAnterior, valor));
            })
            .OrderByDescending(g => g.Valor)
            .ThenByDescending(g => g.ValorMesAnterior)
            .ToList();
    }

    private static decimal Somar(List<Lancamento> lancamentos, TipoLancamento tipo)
        => lancamentos.Where(l => l.Tipo == tipo).Sum(l => l.Valor);

    private static decimal? VariacaoPercentual(decimal anterior, decimal atual)
        => anterior == 0 ? null : Math.Round((atual - anterior) / anterior * 100, 2);
}
