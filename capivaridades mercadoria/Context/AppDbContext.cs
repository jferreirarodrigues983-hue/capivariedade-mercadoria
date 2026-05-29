using capivaridades_mercadoria.Models;
using Microsoft.EntityFrameworkCore;

namespace capivaridades_mercadoria.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Produto> Produtos { get; set; }
    }
}