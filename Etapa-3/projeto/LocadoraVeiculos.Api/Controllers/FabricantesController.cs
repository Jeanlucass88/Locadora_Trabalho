using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using LocadoraVeiculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/fabricantes")]
public class FabricantesController(LocadoraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FabricanteResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.Fabricantes.AsNoTracking().OrderBy(x => x.Nome)
            .Select(x => new FabricanteResponse(x.Id, x.Nome, x.PaisOrigem))
            .ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FabricanteResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await db.Fabricantes.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new FabricanteResponse(x.Id, x.Nome, x.PaisOrigem))
            .SingleOrDefaultAsync(cancellationToken);
        return item is null ? throw ApiException.NotFound("Fabricante", id) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<FabricanteResponse>> Create(FabricanteRequest request, CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        if (await db.Fabricantes.AnyAsync(x => x.Nome == nome, cancellationToken))
            throw ApiException.Conflict("Já existe um fabricante com esse nome.");

        var entity = new Fabricante { Nome = nome, PaisOrigem = request.PaisOrigem?.Trim() };
        db.Fabricantes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var response = new FabricanteResponse(entity.Id, entity.Nome, entity.PaisOrigem);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, FabricanteRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Fabricantes.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Fabricante", id);
        var nome = request.Nome.Trim();
        if (await db.Fabricantes.AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken))
            throw ApiException.Conflict("Já existe um fabricante com esse nome.");

        entity.Nome = nome;
        entity.PaisOrigem = request.PaisOrigem?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Fabricantes.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Fabricante", id);
        if (await db.Veiculos.AnyAsync(x => x.FabricanteId == id, cancellationToken))
            throw ApiException.Conflict("O fabricante não pode ser excluído porque possui veículos vinculados.");

        db.Fabricantes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
