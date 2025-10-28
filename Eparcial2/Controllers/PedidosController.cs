using System.Security.Claims;
using Eparcial2.Data;
using Eparcial2.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace parcial2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PedidosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public PedidosController(AppDbContext context) => _context = context;

        // Crear pedido: list of PedidoProducto (ProductoId + Cantidad)
        public class CrearItemDto { public int ProductoId { get; set; } public int Cantidad { get; set; } }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        public async Task<IActionResult> CrearPedido([FromBody] List<CrearItemDto> items)
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (items == null || !items.Any()) return BadRequest("Pedido vacío");

            var productoIds = items.Select(i => i.ProductoId).Distinct().ToList();
            var productos = await _context.Productos.Where(p => productoIds.Contains(p.Id)).ToListAsync();

            // Todos deben pertenecer a la misma empresa
            var empresaId = productos.First().EmpresaId;
            if (productos.Any(p => p.EmpresaId != empresaId))
                return BadRequest("Todos los productos deben pertenecer a la misma empresa.");

            // verificar stock
            foreach (var it in items)
            {
                var prod = productos.First(p => p.Id == it.ProductoId);
                if (it.Cantidad <= 0) return BadRequest("Cantidad inválida.");
                if (prod.Stock < it.Cantidad) return BadRequest($"Stock insuficiente para {prod.Nombre}");
            }

            // Transacción: crear pedido, descontar stock, crear pedido-producto
            using (var tx = await _context.Database.BeginTransactionAsync())
            {
                var pedido = new Pedido { ClienteId = clienteId, EmpresaId = empresaId };
                _context.Pedidos.Add(pedido);
                await _context.SaveChangesAsync();

                foreach (var it in items)
                {
                    var prod = productos.First(p => p.Id == it.ProductoId);
                    prod.Stock -= it.Cantidad;
                    _context.PedidoProductos.Add(new PedidoProducto
                    {
                        PedidoId = pedido.Id,
                        ProductoId = prod.Id,
                        Cantidad = it.Cantidad,
                        PrecioUnitario = prod.Precio
                    });
                }

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return Ok(pedido);
            }
        }

        [Authorize(Roles = "Cliente")]
        [HttpGet("mis-pedidos")]
        public async Task<IActionResult> MisPedidos()
        {
            var clienteId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var pedidos = await _context.Pedidos
                .Include(p => p.PedidoProductos)
                .ThenInclude(pp => pp.Producto)
                .Where(p => p.ClienteId == clienteId)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
            return Ok(pedidos);
        }

        [Authorize(Roles = "Empresa")]
        [HttpGet("recibidos")]
        public async Task<IActionResult> PedidosRecibidos()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return BadRequest("Empresa no encontrada.");

            var pedidos = await _context.Pedidos
                .Include(p => p.Cliente)
                .Include(p => p.PedidoProductos)
                .ThenInclude(pp => pp.Producto)
                .Where(p => p.EmpresaId == empresa.Id)
                .OrderByDescending(p => p.Fecha)
                .ToListAsync();
            return Ok(pedidos);
        }

        [Authorize(Roles = "Empresa")]
        [HttpPut("{id}/estado")]
        public async Task<IActionResult> CambiarEstado(int id, [FromBody] string nuevoEstado)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return BadRequest("Empresa no encontrada.");

            var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id && p.EmpresaId == empresa.Id);
            if (pedido == null) return NotFound();

            pedido.Estado = nuevoEstado;
            await _context.SaveChangesAsync();
            return Ok(pedido);
        }
    }
}
