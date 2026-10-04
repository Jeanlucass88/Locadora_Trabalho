using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using LocadoraVeiculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/categorias-veiculo")]
public class CategoriasVeiculoController(LocadoraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoriaVeiculoResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await db.CategoriasVeiculo.AsNoTracking().OrderBy(x => x.Nome)
            .Select(x => new CategoriaVeiculoResponse(x.Id, x.Nome, x.Descricao))
            .ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoriaVeiculoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await db.CategoriasVeiculo.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new CategoriaVeiculoResponse(x.Id, x.Nome, x.Descricao))
            .SingleOrDefaultAsync(cancellationToken);
        return item is null ? throw ApiException.NotFound("Categoria", id) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoriaVeiculoResponse>> Create(CategoriaVeiculoRequest request, CancellationToken cancellationToken)
    {
        var nome = request.Nome.Trim();
        if (await db.CategoriasVeiculo.AnyAsync(x => x.Nome == nome, cancellationToken))
            throw ApiException.Conflict("Já existe uma categoria com esse nome.");

        var entity = new CategoriaVeiculo { Nome = nome, Descricao = request.Descricao?.Trim() };
        db.CategoriasVeiculo.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var response = new CategoriaVeiculoResponse(entity.Id, entity.Nome, entity.Descricao);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoriaVeiculoRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.CategoriasVeiculo.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Categoria", id);
        var nome = request.Nome.Trim();
        if (await db.CategoriasVeiculo.AnyAsync(x => x.Id != id && x.Nome == nome, cancellationToken))
            throw ApiException.Conflict("Já existe uma categoria com esse nome.");

        entity.Nome = nome;
        entity.Descricao = request.Descricao?.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.CategoriasVeiculo.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Categoria", id);
        if (await db.Veiculos.AnyAsync(x => x.CategoriaVeiculoId == id, cancellationToken))
            throw ApiException.Conflict("A categoria não pode ser excluída porque possui veículos vinculados.");

        db.CategoriasVeiculo.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
