using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starlink.Api.Data;
using Starlink.Api.Models;

namespace Starlink.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VendasController(StarlinkDbContext db) : ControllerBase
{
    private int DonoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Venda>>> GetAll()
    {
        return await db.Vendas.AsNoTracking()
            .Where(v => v.DonoId == DonoId)
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Venda>> GetById(int id)
    {
        var venda = await db.Vendas.AsNoTracking()
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == id && v.DonoId == DonoId);

        return venda is null ? NotFound() : venda;
    }

    [HttpGet("cliente/{clienteId:int}")]
    public async Task<ActionResult<IEnumerable<Venda>>> GetByCliente(int clienteId)
    {
        var clienteExiste = await db.Clientes.AnyAsync(c => c.Id == clienteId && c.DonoId == DonoId);
        if (!clienteExiste) return NotFound("Cliente não encontrado.");

        return await db.Vendas.AsNoTracking()
            .Where(v => v.ClienteId == clienteId && v.DonoId == DonoId)
            .OrderByDescending(v => v.DataVenda)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Venda>> Create(Venda venda)
    {
        var clienteExiste = await db.Clientes.AnyAsync(c => c.Id == venda.ClienteId && c.DonoId == DonoId);
        if (!clienteExiste) return NotFound("Cliente não encontrado.");

        venda.Id = 0;
        venda.DonoId = DonoId;
        venda.Cliente = null;
        venda.DataVenda = DateTime.UtcNow;

        db.Vendas.Add(venda);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = venda.Id }, venda);
    }

    [HttpPut("{id:int}/status")]
    public async Task<IActionResult> AtualizarStatus(int id, StatusVenda status)
    {
        var venda = await db.Vendas.FirstOrDefaultAsync(v => v.Id == id && v.DonoId == DonoId);
        if (venda is null) return NotFound();

        venda.Status = status;
        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var venda = await db.Vendas.FirstOrDefaultAsync(v => v.Id == id && v.DonoId == DonoId);
        if (venda is null) return NotFound();

        db.Vendas.Remove(venda);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
