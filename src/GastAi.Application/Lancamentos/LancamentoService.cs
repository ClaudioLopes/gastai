using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Application.Orcamentos;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Application.Lancamentos;

public class LancamentoService(
    ILancamentoRepository lancamentos,
    ICategoriaRepository categorias,
    IOrcamentoService orcamentos,
    IUnitOfWork unitOfWork) : ILancamentoService
{
    private const int MaxParcelas = 60;

    public async Task<RegistrarGastoResultDto> RegistrarGastoAsync(CriarGastoRequest request, CancellationToken ct = default)
    {
        var categoria = await ObterCategoriaValidaAsync(request.CategoriaId, TipoLancamento.Gasto, ct);

        if (request.TotalParcelas is > 1)
            return await RegistrarGastoParceladoAsync(request, categoria, ct);

        var lancamento = Lancamento.NovoGasto(
            request.Valor, request.Data, request.Descricao, request.CategoriaId, request.FormaPagamento, request.Observacao);

        await lancamentos.AddAsync(lancamento, ct);
        await unitOfWork.SaveChangesAsync(ct);

        var alertas = await orcamentos.VerificarAlertasAsync(
            request.CategoriaId, categoria.Nome, request.Data.Year, request.Data.Month, ct);

        return new RegistrarGastoResultDto(ParaDto(lancamento, categoria), alertas);
    }

    private async Task<RegistrarGastoResultDto> RegistrarGastoParceladoAsync(
        CriarGastoRequest request, Categoria categoria, CancellationToken ct)
    {
        if (request.FormaPagamento != FormaPagamento.Credito)
            throw new DomainException("Parcelamento só é permitido para pagamento em crédito.");

        var totalParcelas = request.TotalParcelas!.Value;
        if (totalParcelas > MaxParcelas)
            throw new DomainException($"Máximo de {MaxParcelas} parcelas.");

        var grupoParcelamentoId = Guid.NewGuid();
        var valorParcela = Math.Round(request.Valor / totalParcelas, 2, MidpointRounding.ToEven);
        var valorUltimaParcela = request.Valor - valorParcela * (totalParcelas - 1);

        Lancamento? primeiraParcela = null;

        for (var i = 0; i < totalParcelas; i++)
        {
            var valor = i == totalParcelas - 1 ? valorUltimaParcela : valorParcela;
            var data = request.Data.AddMonths(i);
            var descricao = string.IsNullOrWhiteSpace(request.Descricao)
                ? $"Parcela {i + 1}/{totalParcelas}"
                : $"{request.Descricao} ({i + 1}/{totalParcelas})";

            var parcela = Lancamento.NovoGasto(valor, data, descricao, request.CategoriaId, request.FormaPagamento, request.Observacao);
            parcela.MarcarComoParcela(grupoParcelamentoId, i + 1, totalParcelas);

            await lancamentos.AddAsync(parcela, ct);
            primeiraParcela ??= parcela;
        }

        await unitOfWork.SaveChangesAsync(ct);

        var alertas = await orcamentos.VerificarAlertasAsync(
            request.CategoriaId, categoria.Nome, request.Data.Year, request.Data.Month, ct);

        return new RegistrarGastoResultDto(ParaDto(primeiraParcela!, categoria), alertas, totalParcelas);
    }

    public async Task<LancamentoDto> RegistrarReceitaAsync(CriarReceitaRequest request, CancellationToken ct = default)
    {
        var categoria = await ObterCategoriaValidaAsync(request.CategoriaId, TipoLancamento.Receita, ct);

        var lancamento = Lancamento.NovaReceita(
            request.Valor, request.Data, request.Descricao, request.CategoriaId, request.Observacao);

        await lancamentos.AddAsync(lancamento, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ParaDto(lancamento, categoria);
    }

    public async Task<LancamentoDto> AtualizarAsync(int id, AtualizarLancamentoRequest request, CancellationToken ct = default)
    {
        var lancamento = await lancamentos.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Lançamento {id} não encontrado.");

        var categoria = await ObterCategoriaValidaAsync(request.CategoriaId, lancamento.Tipo, ct);

        lancamento.Atualizar(
            request.Valor, request.Data, request.Descricao, request.CategoriaId, request.FormaPagamento, request.Observacao);

        await unitOfWork.SaveChangesAsync(ct);

        return ParaDto(lancamento, categoria);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var lancamento = await lancamentos.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Lançamento {id} não encontrado.");

        lancamentos.Remove(lancamento);
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task<List<LancamentoDto>> ListarPorMesAsync(int ano, int mes, CancellationToken ct = default)
    {
        var lista = await lancamentos.GetByMesAsync(ano, mes, ct);

        return lista
            .OrderByDescending(l => l.Data)
            .ThenByDescending(l => l.Id)
            .Select(l => ParaDto(l, l.Categoria!))
            .ToList();
    }

    private async Task<Domain.Entities.Categoria> ObterCategoriaValidaAsync(int categoriaId, TipoLancamento tipoEsperado, CancellationToken ct)
    {
        var categoria = await categorias.GetByIdAsync(categoriaId, ct)
            ?? throw new NotFoundException($"Categoria {categoriaId} não encontrada.");

        if (categoria.Tipo != tipoEsperado)
            throw new DomainException($"A categoria '{categoria.Nome}' não é do tipo {tipoEsperado}.");

        return categoria;
    }

    private static LancamentoDto ParaDto(Lancamento l, Domain.Entities.Categoria categoria)
        => new(l.Id, l.Tipo, l.Valor, l.Data, l.Descricao, l.CategoriaId, categoria.Nome, l.FormaPagamento, l.Observacao,
            l.NumeroParcela, l.TotalParcelas);
}
