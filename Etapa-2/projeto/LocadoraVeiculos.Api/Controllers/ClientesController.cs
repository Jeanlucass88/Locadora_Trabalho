using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using LocadoraVeiculos.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public class ClientesController(LocadoraDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClienteResponse>>> GetAll(CancellationToken cancellationToken) =>
        Ok(await Query().OrderBy(x => x.Nome).ToListAsync(cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ClienteResponse>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await Query().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return item is null ? throw ApiException.NotFound("Cliente", id) : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ClienteResponse>> Create(ClienteRequest request, CancellationToken cancellationToken)
    {
        var cpf = OnlyDigits(request.Cpf);
        var cnh = OnlyDigits(request.NumeroCnh);
        var email = request.Email.Trim().ToLowerInvariant();
        await ValidateUnique(cpf, email, cnh, null, cancellationToken);

        var entity = new Cliente
        {
            Nome = request.Nome.Trim(),
            Cpf = cpf,
            Email = email,
            Telefone = request.Telefone?.Trim(),
            DataNascimento = request.DataNascimento,
            NumeroCnh = cnh,
            ValidadeCnh = request.ValidadeCnh,
            Ativo = request.Ativo
        };
        db.Clientes.Add(entity);
        await db.SaveChangesAsync(cancellationToken);
        var response = await Query().SingleAsync(x => x.Id == entity.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ClienteRequest request, CancellationToken cancellationToken)
    {
        var entity = await db.Clientes.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Cliente", id);
        var cpf = OnlyDigits(request.Cpf);
        var cnh = OnlyDigits(request.NumeroCnh);
        var email = request.Email.Trim().ToLowerInvariant();
        await ValidateUnique(cpf, email, cnh, id, cancellationToken);

        entity.Nome = request.Nome.Trim();
        entity.Cpf = cpf;
        entity.Email = email;
        entity.Telefone = request.Telefone?.Trim();
        entity.DataNascimento = request.DataNascimento;
        entity.NumeroCnh = cnh;
        entity.ValidadeCnh = request.ValidadeCnh;
        entity.Ativo = request.Ativo;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await db.Clientes.FindAsync([id], cancellationToken)
            ?? throw ApiException.NotFound("Cliente", id);
        if (await db.Alugueis.AnyAsync(x => x.ClienteId == id, cancellationToken))
            throw ApiException.Conflict("O cliente não pode ser excluído porque possui aluguéis vinculados.");

        db.Clientes.Remove(entity);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<ClienteResponse> Query() => db.Clientes.AsNoTracking().Select(x =>
        new ClienteResponse(x.Id, x.Nome, x.Cpf, x.Email, x.Telefone, x.DataNascimento,
            x.NumeroCnh, x.ValidadeCnh, x.Ativo));

    private async Task ValidateUnique(
        string cpf, string email, string cnh, int? currentId, CancellationToken cancellationToken)
    {
        var query = db.Clientes.AsQueryable();
        if (currentId.HasValue)
            query = query.Where(x => x.Id != currentId.Value);

        if (await query.AnyAsync(x => x.Cpf == cpf, cancellationToken))
            throw ApiException.Conflict("Já existe um cliente com esse CPF.");
        if (await query.AnyAsync(x => x.Email == email, cancellationToken))
            throw ApiException.Conflict("Já existe um cliente com esse e-mail.");
        if (await query.AnyAsync(x => x.NumeroCnh == cnh, cancellationToken))
            throw ApiException.Conflict("Já existe um cliente com essa CNH.");
    }

    private static string OnlyDigits(string value) => new(value.Where(char.IsDigit).ToArray());
}
