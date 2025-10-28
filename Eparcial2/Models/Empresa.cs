using Eparcial2.Models;

namespace Eparcial2.Models
{
    public class Empresa
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;

        // Usuario propietario (opcional: referencia al user que administra la empresa)
        public int? OwnerUserId { get; set; }

        // Relaciones
        public List<Producto>? Productos { get; set; }
        public List<Pedido>? Pedidos { get; set; }
    }
}