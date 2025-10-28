namespace Eparcial2.Models
{
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = "";
        public string Descripcion { get; set; } = "";
        public decimal Precio { get; set; }
        public int Stock { get; set; }

        // FK a Empresa
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public List<PedidoProducto>? PedidoProductos { get; set; }
        public List<Reseña>? Resenas { get; set; }
    }
}