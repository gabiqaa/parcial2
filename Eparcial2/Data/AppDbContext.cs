using Eparcial2.Models;
using Microsoft.EntityFrameworkCore;

namespace Eparcial2.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoProducto> PedidoProductos { get; set; }
        public DbSet<Reseña> Resenas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Pedido -> PedidoProductos (1..n)
            modelBuilder.Entity<Pedido>()
                .HasMany(p => p.PedidoProductos)
                .WithOne(pp => pp.Pedido)
                .HasForeignKey(pp => pp.PedidoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Empresa -> Productos (1..n)
            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Productos)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Producto -> Resenas (1..n)
            modelBuilder.Entity<Producto>()
                .HasMany(p => p.Resenas)
                .WithOne(r => r.Producto)
                .HasForeignKey(r => r.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}