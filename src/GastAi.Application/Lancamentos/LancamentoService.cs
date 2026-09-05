using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Contracts.Lancamentos;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Application.Lancamentos;

public class LancamentoService(
    ILancamentoRepository lancamentos,
    ICategoriaRepository categorias,
    IUnitOfWork unitOfWork) : ILancamentoService
{
    public async Task<LancamentoDto> RegistrarGastoAsync(CriarGastoRequest request, CancellationToken ct = default)
    {
        var categoria = await ObterCategoriaValidaAsync(request.CategoriaId, TipoLancamento.Gasto, ct);

        var lancamento = Lancamento.NovoGasto(
            request.Valor, request.Data, request.Descricao, request.CategoriaId, request.FormaPagamento, request.Observacao);

        await lancamentos.AddAsync(lancamento, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ParaDto(lancamento, categoria);
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
        => new(l.Id, l.Tipo, l.Valor, l.Data, l.Descricao, l.CategoriaId, categoria.Nome, l.FormaPagamento, l.Observacao);
}
