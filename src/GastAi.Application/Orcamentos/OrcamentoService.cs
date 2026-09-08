using System.Globalization;
using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Contracts.Orcamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Application.Orcamentos;

public class OrcamentoService(
    IOrcamentoRepository orcamentos,
    ILancamentoRepository lancamentos,
    ICategoriaRepository categorias,
    IUnitOfWork unitOfWork) : IOrcamentoService
{
    private static readonly CultureInfo PtBr = CultureInfo.GetCultureInfo("pt-BR");

    public async Task<List<OrcamentoMensalDto>> ListarAsync(int ano, int mes, CancellationToken ct = default)
    {
        var mesReferencia = new DateOnly(ano, mes, 1);
        var orcamentosDoMes = await orcamentos.GetByMesAsync(mesReferencia, ct);
        var lancamentosDoMes = await lancamentos.GetByMesAsync(ano, mes, ct);

        return orcamentosDoMes
            .Select(o => ParaDto(o, lancamentosDoMes))
            .OrderBy(o => o.CategoriaId is null ? 0 : 1)
            .ThenBy(o => o.CategoriaNome)
            .ToList();
    }

    public async Task<OrcamentoMensalDto> DefinirAsync(DefinirOrcamentoRequest request, CancellationToken ct = default)
    {
        Categoria? categoria = null;
        if (request.CategoriaId is not null)
        {
            categoria = await categorias.GetByIdAsync(request.CategoriaId.Value, ct)
                ?? throw new NotFoundException($"Categoria {request.CategoriaId} não encontrada.");

            if (categoria.Tipo != TipoLancamento.Gasto)
                throw new DomainException("Só é possível definir orçamento para categorias de gasto.");
        }

        var mesReferencia = new DateOnly(request.Ano, request.Mes, 1);
        var orcamento = await orcamentos.GetByCategoriaEMesAsync(request.CategoriaId, mesReferencia, ct);

        if (orcamento is not null)
        {
            orcamento.AtualizarLimite(request.ValorLimite);
        }
        else
        {
            orcamento = new OrcamentoMensal(request.CategoriaId, mesReferencia, request.ValorLimite);
            await orcamentos.AddAsync(orcamento, ct);
        }

        await unitOfWork.SaveChangesAsync(ct);

        var lancamentosDoMes = await lancamentos.GetByMesAsync(request.Ano, request.Mes, ct);
        return ParaDto(orcamento, lancamentosDoMes, categoria?.Nome);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var orcamento = await orcamentos.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Orçamento {id} não encontrado.");

        orcamentos.Remove(orcamento);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<List<string>> VerificarAlertasAsync(
        int categoriaId, string categoriaNome, int ano, int mes, CancellationToken ct = default)
    {
        var mesReferencia = new DateOnly(ano, mes, 1);
        var lancamentosDoMes = await lancamentos.GetByMesAsync(ano, mes, ct);
        var alertas = new List<string>();

        var orcamentoCategoria = await orcamentos.GetByCategoriaEMesAsync(categoriaId, mesReferencia, ct);
        if (orcamentoCategoria is not null)
        {
            var gastoCategoria = Somar(lancamentosDoMes, categoriaId);
            if (gastoCategoria > orcamentoCategoria.ValorLimite)
                alertas.Add(
                    $"Orçamento de {categoriaNome} estourado: {Moeda(gastoCategoria)} de {Moeda(orcamentoCategoria.ValorLimite)}.");
        }

        var orcamentoGeral = await orcamentos.GetByCategoriaEMesAsync(null, mesReferencia, ct);
        if (orcamentoGeral is not null)
        {
            var gastoGeral = Somar(lancamentosDoMes, categoriaId: null);
            if (gastoGeral > orcamentoGeral.ValorLimite)
                alertas.Add($"Orçamento geral do mês estourado: {Moeda(gastoGeral)} de {Moeda(orcamentoGeral.ValorLimite)}.");
        }

        return alertas;
    }

    private static decimal Somar(List<Lancamento> lancamentosDoMes, int? categoriaId)
        => lancamentosDoMes
            .Where(l => l.Tipo == TipoLancamento.Gasto && (categoriaId is null || l.CategoriaId == categoriaId))
            .Sum(l => l.Valor);

    private static OrcamentoMensalDto ParaDto(OrcamentoMensal o, List<Lancamento> lancamentosDoMes, string? categoriaNome = null)
    {
        var valorGasto = Somar(lancamentosDoMes, o.CategoriaId);
        var nome = o.CategoriaId is null ? "Geral" : categoriaNome ?? o.Categoria?.Nome ?? "Categoria";
        var percentual = o.ValorLimite == 0 ? 0 : Math.Round(valorGasto / o.ValorLimite * 100, 2);

        return new OrcamentoMensalDto(
            o.Id, o.CategoriaId, nome, o.MesReferencia.Year, o.MesReferencia.Month, o.ValorLimite, valorGasto, percentual);
    }

    private static string Moeda(decimal valor) => valor.ToString("C2", PtBr);
}
