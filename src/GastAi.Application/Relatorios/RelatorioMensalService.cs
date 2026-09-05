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

        var gastosPorCategoria = doMes
            .Where(l => l.Tipo == TipoLancamento.Gasto)
            .GroupBy(l => l.Categoria!)
            .Select(g => new GastoPorCategoriaDto(
                g.Key.Id,
                g.Key.Nome,
                g.Sum(l => l.Valor),
                totalGastos == 0 ? 0 : Math.Round(g.Sum(l => l.Valor) / totalGastos * 100, 2)))
            .OrderByDescending(g => g.Valor)
            .ToList();

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

    private static decimal Somar(List<Lancamento> lancamentos, TipoLancamento tipo)
        => lancamentos.Where(l => l.Tipo == tipo).Sum(l => l.Valor);

    private static decimal? VariacaoPercentual(decimal anterior, decimal atual)
        => anterior == 0 ? null : Math.Round((atual - anterior) / anterior * 100, 2);
}
