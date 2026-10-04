using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using LocadoraVeiculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/veiculos")]
public class VeiculosController(LocadoraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<VeiculoResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await Query().OrderBy(x => x.Modelo).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VeiculoResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item is null ? throw ApiException.NotFound("Veículo", id) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<VeiculoResponse>> Create(VeiculoRequest request, CancellationToken cancellationToken)
    {
        await ValidateReferences(request.FabricanteId, request.CategoriaVeiculoId, cancellationToken);
        var placa = request.Placa.Trim().ToUpperInvariant();
        var chassi = request.Chassi.Trim().ToUpperInvariant();
        await ValidateUnique(placa, chassi, null, cancellationToken);

        var entity = new Veiculo
        {
            FabricanteId = request.FabricanteId,
            CategoriaVeiculoId = request.CategoriaVeiculoId,
            Modelo = request.Modelo.Trim(),
            Placa = placa,
            Chassi = chassi,
            AnoFabricacao = request.AnoFabricacao,
            Quilometragem = request.Quilometragem,
            Ativo = request.Ativo
        };
        db.Veiculos.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var response = await Query().SingleAsync(x => x.Id == entity.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, VeiculoRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Veiculos.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Veículo", id);
        await ValidateReferences(request.FabricanteId, request.CategoriaVeiculoId, cancellationToken);
        var placa = request.Placa.Trim().ToUpperInvariant();
        var chassi = request.Chassi.Trim().ToUpperInvariant();
        await ValidateUnique(placa, chassi, id, cancellationToken);

        entity.FabricanteId = request.FabricanteId;
        entity.CategoriaVeiculoId = request.CategoriaVeiculoId;
        entity.Modelo = request.Modelo.Trim();
        entity.Placa = placa;
        entity.Chassi = chassi;
        entity.AnoFabricacao = request.AnoFabricacao;
        entity.Quilometragem = request.Quilometragem;
        entity.Ativo = request.Ativo;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Veiculos.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Veículo", id);
        if (await db.Alugueis.AnyAsync(x => x.VeiculoId == id, cancellationToken))
            throw ApiException.Conflict("O veículo não pode ser excluído porque possui aluguéis vinculados.");

        db.Veiculos.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<VeiculoResponse> Query() => db.Veiculos.AsNoTracking().Select(x =>
        new VeiculoResponse(x.Id, x.FabricanteId, x.Fabricante.Nome, x.CategoriaVeiculoId,
            x.CategoriaVeiculo.Nome, x.Modelo, x.Placa, x.Chassi, x.AnoFabricacao,
            x.Quilometragem, x.Ativo));

    private async Task ValidateReferences(int fabricanteId, int categoriaId, CancellationToken cancellationToken)
    {
        if (!await db.Fabricantes.AnyAsync(x => x.Id == fabricanteId, cancellationToken))
            throw ApiException.NotFound("Fabricante", fabricanteId);
        if (!await db.CategoriasVeiculo.AnyAsync(x => x.Id == categoriaId, cancellationToken))
            throw ApiException.NotFound("Categoria", categoriaId);
    }

    private async Task ValidateUnique(string placa, string chassi, int? currentId, CancellationToken cancellationToken)
    {
        var query = db.Veiculos.AsQueryable();
        if (currentId.HasValue)
            query = query.Where(x => x.Id != currentId.Value);

        if (await query.AnyAsync(x => x.Placa == placa, cancellationToken))
            throw ApiException.Conflict("Já existe um veículo com essa placa.");
        if (await query.AnyAsync(x => x.Chassi == chassi, cancellationToken))
            throw ApiException.Conflict("Já existe um veículo com esse chassi.");
    }
}
