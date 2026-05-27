using System.Text.Json;
using capivaridades_mercadoria.Models;

namespace capivaridades_mercadoria.Services
{
    public class JsonProdutoStore : IProdutoStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true
        };

        private readonly string _filePath;
        private readonly string _legacyFilePath;
        private readonly SemaphoreSlim _lock = new(1, 1);

        public JsonProdutoStore(IWebHostEnvironment environment)
        {
            var dataDir = Path.Combine(environment.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "produtos.json");
            _legacyFilePath = Path.Combine(dataDir, "produto.json");
        }

        public async Task<IReadOnlyList<Produto>> ListarAsync()
        {
            var produtos = await CarregarAsync();
            return produtos.OrderBy(p => p.Id).ToList();
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            var produtos = await CarregarAsync();
            return produtos.FirstOrDefault(p => p.Id == id);
        }

        public async Task SalvarAsync(Produto produto)
        {
            await _lock.WaitAsync();
            try
            {
                var produtos = await LerArquivoSemLockAsync();
                var indice = produtos.FindIndex(p => p.Id == produto.Id);

                if (indice < 0)
                {
                    return;
                }

                var atual = produtos[indice];
                atual.Nome = produto.Nome;
                atual.Preco = produto.Preco;

                await GravarArquivoAsync(produtos);
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<Produto> AdicionarAsync(Produto produto)
        {
            await _lock.WaitAsync();
            try
            {
                var produtos = File.Exists(_filePath)
                    ? await LerArquivoSemLockAsync()
                    : await MigrarOuCriarCatalogoAsync();

                produto.Id = produtos.Count > 0 ? produtos.Max(p => p.Id) + 1 : 1;

                if (string.IsNullOrWhiteSpace(produto.Categoria))
                {
                    produto.Categoria = "Celular";
                }

                if (string.IsNullOrWhiteSpace(produto.ImagemUrl))
                {
                    produto.ImagemUrl = new Produto().ImagemUrl;
                }

                produtos.Add(produto);
                await GravarArquivoAsync(produtos);
                return produto;
            }
            finally
            {
                _lock.Release();
            }
        }

        public async Task<bool> ExcluirAsync(int id)
        {
            await _lock.WaitAsync();
            try
            {
                if (!File.Exists(_filePath))
                {
                    return false;
                }

                var produtos = await LerArquivoSemLockAsync();
                var produto = produtos.FirstOrDefault(p => p.Id == id);

                if (produto is null || produtos.Count <= 1)
                {
                    return false;
                }

                produtos.Remove(produto);
                await GravarArquivoAsync(produtos);
                return true;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Produto>> CarregarAsync()
        {
            await _lock.WaitAsync();
            try
            {
                if (File.Exists(_filePath))
                {
                    return await LerArquivoSemLockAsync();
                }

                var produtos = await MigrarOuCriarCatalogoAsync();
                await GravarArquivoAsync(produtos);
                return produtos;
            }
            finally
            {
                _lock.Release();
            }
        }

        private async Task<List<Produto>> MigrarOuCriarCatalogoAsync()
        {
            if (File.Exists(_legacyFilePath))
            {
                await using var stream = File.OpenRead(_legacyFilePath);
                var legado = await JsonSerializer.DeserializeAsync<Produto>(stream, JsonOptions);

                if (legado is not null)
                {
                    legado.Id = 1;
                    if (string.IsNullOrWhiteSpace(legado.Categoria))
                    {
                        legado.Categoria = "Celular";
                    }

                    var catalogo = CriarCatalogoPadrao();
                    catalogo[0] = legado;

                    for (var i = 1; i < catalogo.Count; i++)
                    {
                        catalogo[i].Id = i + 1;
                    }

                    return catalogo;
                }
            }

            return CriarCatalogoPadrao();
        }

        private static List<Produto> CriarCatalogoPadrao()
        {
            return
            [
                new Produto
                {
                    Id = 1,
                    Nome = "iPhone 15 Pro Max",
                    Preco = "R$ 3.500,00",
                    Categoria = "Celular",
                    ImagemUrl = "https://trocafone.vtexassets.com/arquivos/ids/297354-800-450?v=638410112977470000&width=800&height=450&aspect=true"
                },
                new Produto
                {
                    Id = 2,
                    Nome = "Samsung Galaxy S24 Ultra",
                    Preco = "R$ 2.899,00",
                    Categoria = "Celular",
                    ImagemUrl = "https://images.unsplash.com/photo-1610945415295-d9bbf080e59c?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 3,
                    Nome = "Xiaomi Redmi Note 13",
                    Preco = "R$ 1.299,00",
                    Categoria = "Celular",
                    ImagemUrl = "https://images.unsplash.com/photo-1598327105666-5b89351affd3?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 4,
                    Nome = "Motorola Edge 40",
                    Preco = "R$ 1.799,00",
                    Categoria = "Celular",
                    ImagemUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 5,
                    Nome = "PlayStation 5",
                    Preco = "R$ 3.999,00",
                    Categoria = "Videogame",
                    ImagemUrl = "https://images.unsplash.com/photo-1606813907291-d86efa9b94db?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 6,
                    Nome = "Xbox Series X",
                    Preco = "R$ 3.699,00",
                    Categoria = "Videogame",
                    ImagemUrl = "https://images.unsplash.com/photo-1621259182978-fbf931f1d3f2?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 7,
                    Nome = "Nintendo Switch OLED",
                    Preco = "R$ 2.199,00",
                    Categoria = "Videogame",
                    ImagemUrl = "https://images.unsplash.com/photo-1578303512597-81e189faa184?w=800&h=450&fit=crop"
                },
                new Produto
                {
                    Id = 8,
                    Nome = "Steam Deck OLED",
                    Preco = "R$ 4.299,00",
                    Categoria = "Videogame",
                    ImagemUrl = "https://images.unsplash.com/photo-1486401896868-4c46bed1f320?w=800&h=450&fit=crop"
                }
            ];
        }

        private async Task<List<Produto>> LerArquivoSemLockAsync()
        {
            await using var stream = File.OpenRead(_filePath);
            return await JsonSerializer.DeserializeAsync<List<Produto>>(stream, JsonOptions)
                ?? CriarCatalogoPadrao();
        }

        private async Task GravarArquivoAsync(List<Produto> produtos)
        {
            await using var stream = File.Create(_filePath);
            await JsonSerializer.SerializeAsync(stream, produtos, JsonOptions);
        }
    }
}
