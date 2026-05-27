using capivaridades_mercadoria.Models;

namespace capivaridades_mercadoria.Services
{
    public interface IProdutoStore
    {
        Task<IReadOnlyList<Produto>> ListarAsync();

        Task<Produto?> ObterPorIdAsync(int id);

        Task SalvarAsync(Produto produto);

        Task<bool> ExcluirAsync(int id);
    }
}
