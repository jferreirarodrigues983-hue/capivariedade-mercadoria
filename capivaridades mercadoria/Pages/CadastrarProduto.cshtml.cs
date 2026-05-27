using System.ComponentModel.DataAnnotations;
using capivaridades_mercadoria.Models;
using capivaridades_mercadoria.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace capivaridades_mercadoria.Pages
{
    public class CadastrarProdutoModel : PageModel
    {
        private readonly IProdutoStore _produtoStore;

        public CadastrarProdutoModel(IProdutoStore produtoStore)
        {
            _produtoStore = produtoStore;
        }

        [BindProperty]
        [Required(ErrorMessage = "Informe o nome do produto.")]
        [Display(Name = "Nome")]
        public string Nome { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Informe o preço.")]
        [Display(Name = "Preço")]
        public string Preco { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Selecione a categoria.")]
        [Display(Name = "Categoria")]
        public string Categoria { get; set; } = "Celular";

        [BindProperty]
        [Display(Name = "URL da imagem")]
        public string? ImagemUrl { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!string.IsNullOrWhiteSpace(ImagemUrl) &&
                !Uri.TryCreate(ImagemUrl.Trim(), UriKind.Absolute, out _))
            {
                ModelState.AddModelError(
                    nameof(ImagemUrl),
                    "Informe uma URL válida para a imagem.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var novo = new Produto
            {
                Nome = Nome.Trim(),
                Preco = Preco.Trim(),
                Categoria = Categoria.Trim(),
                ImagemUrl = string.IsNullOrWhiteSpace(ImagemUrl)
                    ? new Produto().ImagemUrl
                    : ImagemUrl.Trim()
            };

            await _produtoStore.AdicionarAsync(novo);

            TempData["MensagemSucesso"] = $"Produto \"{novo.Nome}\" cadastrado com sucesso!";
            return RedirectToPage();
        }
    }
}
