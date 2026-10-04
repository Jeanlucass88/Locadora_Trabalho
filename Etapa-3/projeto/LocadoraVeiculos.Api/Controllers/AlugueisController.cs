using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using LocadoraVeiculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/alugueis")]
public class AlugueisController(LocadoraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AluguelResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await Query().OrderByDescending(x => x.DataRetirada).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AluguelResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item is null ? throw ApiException.NotFound("Aluguel", id) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<AluguelResponse>> Create(AluguelCreateRequest request, CancellationToken cancellationToken)
    {
        var (_, veiculo) = await ValidateReferences(request.ClienteId, request.VeiculoId, cancellationToken);
        if (request.QuilometragemInicial != veiculo.Quilometragem)
            throw ApiException.Conflict($"A quilometragem inicial deve ser igual à atual do veículo ({veiculo.Quilometragem} km).");
        await ValidateAvailability(request.VeiculoId, request.DataRetirada,
            request.DataPrevistaDevolucao, null, cancellationToken);

        var entity = new Aluguel
        {
            ClienteId = request.ClienteId,
            VeiculoId = request.VeiculoId,
            DataRetirada = request.DataRetirada,
            DataPrevistaDevolucao = request.DataPrevistaDevolucao,
            QuilometragemInicial = request.QuilometragemInicial,
            ValorDiaria = request.ValorDiaria
        };
        db.Alugueis.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var response = await Query().SingleAsync(x => x.Id == entity.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, AluguelUpdateRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Alugueis.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Aluguel", id);
        var (_, veiculo) = await ValidateReferences(request.ClienteId, request.VeiculoId, cancellationToken);

        if (entity.DataDevolucao is not null && request.DataDevolucao is null)
            throw ApiException.Conflict("Um aluguel já devolvido não pode ser reaberto.");
        if (entity.DataDevolucao is not null && entity.VeiculoId != request.VeiculoId)
            throw ApiException.Conflict("O veículo de um aluguel já devolvido não pode ser alterado.");
        if (entity.VeiculoId != request.VeiculoId && request.QuilometragemInicial != veiculo.Quilometragem)
            throw ApiException.Conflict($"A quilometragem inicial deve ser igual à atual do veículo ({veiculo.Quilometragem} km).");

        await ValidateAvailability(request.VeiculoId, request.DataRetirada,
            request.DataDevolucao ?? request.DataPrevistaDevolucao, id, cancellationToken);

        entity.ClienteId = request.ClienteId;
        entity.VeiculoId = request.VeiculoId;
        entity.DataRetirada = request.DataRetirada;
        entity.DataPrevistaDevolucao = request.DataPrevistaDevolucao;
        entity.DataDevolucao = request.DataDevolucao;
        entity.QuilometragemInicial = request.QuilometragemInicial;
        entity.QuilometragemFinal = request.QuilometragemFinal;
        entity.ValorDiaria = request.ValorDiaria;
        entity.ValorTotal = request.ValorTotal;

        if (request.QuilometragemFinal.HasValue)
        {
            if (request.QuilometragemFinal.Value < veiculo.Quilometragem)
                throw ApiException.Conflict($"A quilometragem final não pode ser menor que a atual do veículo ({veiculo.Quilometragem} km).");
            veiculo.Quilometragem = request.QuilometragemFinal.Value;
        }

        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Alugueis.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Aluguel", id);
        db.Alugueis.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<AluguelResponse> Query() => db.Alugueis.AsNoTracking().Select(x =>
        new AluguelResponse(x.Id, x.ClienteId, x.Cliente.Nome, x.VeiculoId,
            x.Veiculo.Fabricante.Nome + " " + x.Veiculo.Modelo, x.Veiculo.Placa,
            x.DataRetirada, x.DataPrevistaDevolucao, x.DataDevolucao,
            x.QuilometragemInicial, x.QuilometragemFinal, x.ValorDiaria, x.ValorTotal,
            x.DataDevolucao != null ? "Devolvido" : "Em aberto"));

    private async Task<(Cliente Cliente, Veiculo Veiculo)> ValidateReferences(
        int clienteId, int veiculoId, CancellationToken cancellationToken)
    {
        var cliente = await db.Clientes.FindAsync([clienteId], cancellationToken)
            ?? throw ApiException.NotFound("Cliente", clienteId);
        var veiculo = await db.Veiculos.FindAsync([veiculoId], cancellationToken)
            ?? throw ApiException.NotFound("Veículo", veiculoId);
        if (!cliente.Ativo)
            throw ApiException.Conflict("O cliente está inativo.");
        if (!veiculo.Ativo)
            throw ApiException.Conflict("O veículo está inativo.");
        if (cliente.ValidadeCnh < DateOnly.FromDateTime(DateTime.Today))
            throw ApiException.Conflict("A CNH do cliente está vencida.");
        return (cliente, veiculo);
    }

    private async Task ValidateAvailability(
        int veiculoId, DateTime inicio, DateTime fim, int? currentId, CancellationToken cancellationToken)
    {
        var query = db.Alugueis.Where(x => x.VeiculoId == veiculoId);
        if (currentId.HasValue)
            query = query.Where(x => x.Id != currentId.Value);

        var conflito = await query.AnyAsync(x =>
            x.DataRetirada < fim && (x.DataDevolucao ?? x.DataPrevistaDevolucao) > inicio,
            cancellationToken);
        if (conflito)
            throw ApiException.Conflict("O veículo já possui um aluguel que se sobrepõe ao período informado.");
    }
}
