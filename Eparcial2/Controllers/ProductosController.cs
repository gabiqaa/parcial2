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
    public class ProductosController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ProductosController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] int? empresaId, [FromQuery] decimal? min, [FromQuery] decimal? max)
        {
            var q = _context.Productos.Include(p => p.Empresa).AsQueryable();
            if (empresaId.HasValue) q = q.Where(p => p.EmpresaId == empresaId);
            if (min.HasValue) q = q.Where(p => p.Precio >= min.Value);
            if (max.HasValue) q = q.Where(p => p.Precio <= max.Value);
            var list = await q.ToListAsync();
            return Ok(list);
        }

        [Authorize(Roles = "Empresa")]
        [HttpGet("mis-productos")]
        public async Task<IActionResult> MisProductos()
        {
            // asumimos que el user.Id representa la empresa owner (si usas empresa separada, adapta)
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            // buscar empresa por OwnerUserId
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return NotFound("Empresa no encontrada para este usuario.");
            var prods = await _context.Productos.Where(p => p.EmpresaId == empresa.Id).ToListAsync();
            return Ok(prods);
        }

        [Authorize(Roles = "Empresa")]
        [HttpPost]
        public async Task<IActionResult> Crear(Producto p)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return BadRequest("Usuario empresa sin empresa asociada.");
            p.EmpresaId = empresa.Id;
            _context.Productos.Add(p);
            await _context.SaveChangesAsync();
            return Ok(p);
        }

        [Authorize(Roles = "Empresa")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Editar(int id, Producto dto)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return BadRequest("Usuario empresa sin empresa asociada.");

            var prod = await _context.Productos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresa.Id);
            if (prod == null) return NotFound();

            prod.Nombre = dto.Nombre;
            prod.Descripcion = dto.Descripcion;
            prod.Precio = dto.Precio;
            prod.Stock = dto.Stock;
            await _context.SaveChangesAsync();
            return Ok(prod);
        }

        [Authorize(Roles = "Empresa")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var empresa = await _context.Empresas.FirstOrDefaultAsync(e => e.OwnerUserId == userId);
            if (empresa == null) return BadRequest("Usuario empresa sin empresa asociada.");

            var prod = await _context.Productos.FirstOrDefaultAsync(x => x.Id == id && x.EmpresaId == empresa.Id);
            if (prod == null) return NotFound();
            _context.Productos.Remove(prod);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
