namespace capivaridades_mercadoria.Models
{
    public class Produto
    {
        public int Id { get; set; }

        public string Nome { get; set; } = "produto";

        public string Preco { get; set; } = "preço";

        public string Categoria { get; set; } = "Celular";

        public string ImagemUrl { get; set; } =
            "https://trocafone.vtexassets.com/arquivos/ids/297354-800-450?v=638410112977470000&width=800&height=450&aspect=true";
    }
}
