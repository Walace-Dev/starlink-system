using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Starlink.Api.Data;
using Starlink.Api.Models;
using Starlink.Api.Validation;

namespace Starlink.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ClientesController(StarlinkDbContext db) : ControllerBase
{
    private int DonoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
    {
        return await db.Clientes.AsNoTracking()
            .Where(c => c.DonoId == DonoId)
            .OrderBy(c => c.Nome)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await db.Clientes.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id && c.DonoId == DonoId);
        return cliente is null ? NotFound() : cliente;
    }

    [HttpPost]
    public async Task<ActionResult<Cliente>> Create(Cliente cliente)
    {
        cliente.Documento = CpfCnpjValidator.OnlyDigits(cliente.Documento);

        if (await db.Clientes.AnyAsync(c => c.DonoId == DonoId && c.Documento == cliente.Documento))
            return Conflict($"Já existe um cliente cadastrado com o documento '{cliente.Documento}'.");

        cliente.Id = 0;
        cliente.DonoId = DonoId;
        cliente.DataCadastro = DateTime.UtcNow;

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.Id }, cliente);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Cliente cliente)
    {
        if (id != cliente.Id) return BadRequest();

        var existente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.DonoId == DonoId);
        if (existente is null) return NotFound();

        cliente.Documento = CpfCnpjValidator.OnlyDigits(cliente.Documento);

        var documentoDuplicado = await db.Clientes
            .AnyAsync(c => c.DonoId == DonoId && c.Documento == cliente.Documento && c.Id != id);
        if (documentoDuplicado)
            return Conflict($"Já existe um cliente cadastrado com o documento '{cliente.Documento}'.");

        existente.Nome = cliente.Nome;
        existente.Documento = cliente.Documento;
        existente.Telefone = cliente.Telefone;
        existente.Email = cliente.Email;
        existente.Endereco = cliente.Endereco;

        await db.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.Id == id && c.DonoId == DonoId);
        if (cliente is null) return NotFound();

        var possuiVendas = await db.Vendas.AnyAsync(v => v.ClienteId == id);
        if (possuiVendas)
            return Conflict("Não é possível excluir um cliente que possui vendas registradas.");

        db.Clientes.Remove(cliente);
        await db.SaveChangesAsync();

        return NoContent();
    }
}
