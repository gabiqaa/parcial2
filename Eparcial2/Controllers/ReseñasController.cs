using System.Security.Claims;
using Eparcial2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Eparcial2.Data;

namespace parcial2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResenasController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ResenasController(AppDbContext context) => _context = context;

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> CrearResena(Reseña r)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

            // validar calificacion entre 1 y 5
            if (r.Calificacion < 1 || r.Calificacion > 5) return BadRequest("Calificación debe ser entre 1 y 5.");

            // verificar que el cliente compró el producto (en cualquier pedido)
            var compro = await _context.PedidoProductos
                .Include(pp => pp.Pedido)
                .AnyAsync(pp => pp.ProductoId == r.ProductoId && pp.Pedido.ClienteId == clienteId);

            if (!compro) return BadRequest("No puedes reseñar un producto que no compraste.");

            r.ClienteId = clienteId;
            r.Fecha = DateTime.UtcNow;
            _context.Resenas.Add(r);
            await _context.SaveChangesAsync();
            return Ok(r);
        }

        [HttpGet("producto/{productoId}")]
        public async Task<IActionResult> GetPorProducto(int productoId)
        {
            var res = await _context.Resenas
                .Include(r => r.Cliente)
                .Where(r => r.ProductoId == productoId)
                .ToListAsync();
            return Ok(res);
        }
    }
}