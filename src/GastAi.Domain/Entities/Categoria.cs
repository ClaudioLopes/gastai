using GastAi.Domain.Enums;
using GastAi.Domain.Exceptions;

namespace GastAi.Domain.Entities;

public class Categoria
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public TipoLancamento Tipo { get; private set; }
    public string? Cor { get; private set; }
    public bool PadraoDoSistema { get; private set; }
    public bool Ativa { get; private set; } = true;

    private Categoria()
    {
    }

    public Categoria(string nome, TipoLancamento tipo, string? cor = null, bool padraoDoSistema = false)
    {
        AtualizarNome(nome);
        Tipo = tipo;
        Cor = cor;
        PadraoDoSistema = padraoDoSistema;
    }

    public void AtualizarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new DomainException("O nome da categoria é obrigatório.");

        Nome = nome.Trim();
    }

    public void AtualizarCor(string? cor) => Cor = cor;

    public void Desativar()
    {
        Ativa = false;
    }

    public void Reativar()
    {
        Ativa = true;
    }

    public void Excluir()
    {
        if (PadraoDoSistema)
            throw new DomainException("Categorias padrão do sistema não podem ser excluídas, apenas desativadas.");
    }
}
