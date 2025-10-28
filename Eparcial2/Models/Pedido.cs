using System;
using System.Collections.Generic;
using Eparcial2.Models;
using Eparcial2.Models;

namespace Eparcial2.Models
{
    public class Pedido
    {
        public int Id { get; set; }

        // Cliente
        public int ClienteId { get; set; }
        public User? Cliente { get; set; }

        // Empresa que vende
        public int EmpresaId { get; set; }
        public Empresa? Empresa { get; set; }

        public DateTime Fecha { get; set; } = DateTime.UtcNow;
        public string Estado { get; set; } = "Nuevo";

        public List<PedidoProducto>? PedidoProductos { get; set; }
    }
}