using GastAi.Application.Abstractions;
using GastAi.Application.Exceptions;
using GastAi.Contracts.Categorias;
using GastAi.Domain.Entities;
using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Application.Categorias;

public class CategoriaService(ICategoriaRepository categorias, IUnitOfWork unitOfWork) : ICategoriaService
{
    public async Task<List<CategoriaDto>> ListarAsync(TipoLancamento? tipo, bool incluirInativas, CancellationToken ct = default)
    {
        var lista = await categorias.GetAllAsync(incluirInativas, ct);

        return lista
            .Where(c => tipo is null || c.Tipo == tipo)
            .OrderBy(c => c.Nome)
            .Select(ParaDto)
            .ToList();
    }

    public async Task<CategoriaDto> CriarAsync(CriarCategoriaRequest request, CancellationToken ct = default)
    {
        if (await categorias.ExisteComNomeAsync(request.Nome, request.Tipo, ct))
            throw new DomainException($"Já existe uma categoria de {request.Tipo} com o nome '{request.Nome}'.");

        var categoria = new Categoria(request.Nome, request.Tipo, request.Cor, padraoDoSistema: false);

        await categorias.AddAsync(categoria, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return ParaDto(categoria);
    }

    public async Task DesativarAsync(int id, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(id, ct);
        categoria.Desativar();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ReativarAsync(int id, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(id, ct);
        categoria.Reativar();
        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ExcluirAsync(int id, CancellationToken ct = default)
    {
        var categoria = await ObterOuFalharAsync(id, ct);
        categoria.Excluir();
        categorias.Remove(categoria);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Categoria> ObterOuFalharAsync(int id, CancellationToken ct)
        => await categorias.GetByIdAsync(id, ct)
           ?? throw new NotFoundException($"Categoria {id} não encontrada.");

    private static CategoriaDto ParaDto(Categoria c)
        => new(c.Id, c.Nome, c.Tipo, c.Cor, c.PadraoDoSistema, c.Ativa);
}
