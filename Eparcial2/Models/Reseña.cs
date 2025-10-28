using System;
using Eparcial2.Models;

namespace Eparcial2.Models
{
    public class Reseña
    {
        public int Id { get; set; }

        public int ClienteId { get; set; }
        public User? Cliente { get; set; }

        public int ProductoId { get; set; }
        public Producto? Producto { get; set; }

        public int Calificacion { get; set; } // 1..5
        public string? Comentario { get; set; }
        public DateTime Fecha { get; set; } = DateTime.UtcNow;
    }
}