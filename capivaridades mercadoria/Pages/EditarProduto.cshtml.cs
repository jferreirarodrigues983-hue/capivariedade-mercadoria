using System.ComponentModel.DataAnnotations;
using capivaridades_mercadoria.Models;
using capivaridades_mercadoria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace capivaridades_mercadoria.Pages
{
    public class EditarProdutoModel : PageModel
    {
        private readonly IProdutoStore _produtoStore;

        public EditarProdutoModel(IProdutoStore produtoStore)
        {
            _produtoStore = produtoStore;
        }

        public bool ModoSelecao { get; private set; }

        public IReadOnlyList<Produto> Produtos { get; private set; } = [];

        public string Categoria { get; private set; } = string.Empty;

        [BindProperty]
        public int ProdutoId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Informe o nome do produto.")]
        [Display(Name = "Nome do produto")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Informe o preço.")]
        [Display(Name = "Preço")]
        public string Preco { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id is null)
            {
                ModoSelecao = true;
                Produtos = await _produtoStore.ListarAsync();
                return Page();
            }

            var produto = await _produtoStore.ObterPorIdAsync(id.Value);
            if (produto is null)
            {
                return NotFound();
            }

            PreencherFormulario(produto);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                var produtoAtual = await _produtoStore.ObterPorIdAsync(ProdutoId);
                Categoria = produtoAtual?.Categoria ?? string.Empty;
                return Page();
            }

            var produto = await _produtoStore.ObterPorIdAsync(ProdutoId);
            if (produto is null)
            {
                return NotFound();
            }

            produto.Nome = Nome.Trim();
            produto.Preco = Preco.Trim();

            await _produtoStore.SalvarAsync(produto);

            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostExcluirAsync(int id)
        {
            var removido = await _produtoStore.ExcluirAsync(id);
            if (!removido)
            {
                TempData["MensagemErro"] = "Não foi possível excluir. É necessário manter pelo menos um produto no catálogo.";
                return RedirectToPage(new { id });
            }

            return RedirectToPage("/Index");
        }

        private void PreencherFormulario(Produto produto)
        {
            ProdutoId = produto.Id;
            Nome = produto.Nome;
            Preco = produto.Preco;
            Categoria = produto.Categoria;
        }
    }
}
