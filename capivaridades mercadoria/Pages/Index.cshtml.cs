using capivaridades_mercadoria.Models;
using capivaridades_mercadoria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace capivaridades_mercadoria.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IProdutoStore _produtoStore;

        public IndexModel(IProdutoStore produtoStore)
        {
            _produtoStore = produtoStore;
        }

        public IReadOnlyList<Produto> Produtos { get; private set; } = [];

        [BindProperty(SupportsGet = true)]
        public string? Busca { get; set; }

        public bool PesquisaAtiva => !string.IsNullOrWhiteSpace(Busca);

        public async Task OnGetAsync()
        {
            await CarregarProdutosAsync();
        }

        public async Task<IActionResult> OnPostExcluirAsync(int id)
        {
            var removido = await _produtoStore.ExcluirAsync(id);
            if (!removido)
            {
                TempData["MensagemErro"] = "Não foi possível excluir. É necessário manter pelo menos um produto no catálogo.";
            }

            return RedirectToPage(new { busca = Busca });
        }

        private async Task CarregarProdutosAsync()
        {
            var todos = await _produtoStore.ListarAsync();

            if (!PesquisaAtiva)
            {
                Produtos = todos;
                return;
            }

            var termo = Busca!.Trim();
            Produtos = todos
                .Where(p =>
                    p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                    p.Preco.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                    p.Categoria.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}
