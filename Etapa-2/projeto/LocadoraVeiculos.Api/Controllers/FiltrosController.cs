using System.ComponentModel.DataAnnotations;
using LocadoraVeiculos.Api.Data;
using LocadoraVeiculos.Api.Dtos;
using LocadoraVeiculos.Api.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LocadoraVeiculos.Api.Controllers;

[ApiController]
[Route("api/filtros")]
public class FiltrosController(LocadoraDbContext db) : ControllerBase
{
    /// <summary>Filtro 1: veículos por fabricante (INNER JOIN entre Veículos, Fabricantes e Categorias).</summary>
    [HttpGet("veiculos-por-fabricante")]
    public async Task<ActionResult<IEnumerable<VeiculoFiltroResponse>>> VeiculosPorFabricante(
        [FromQuery, Required, MinLength(2)] string nome,
        CancellationToken cancellationToken)
    {
        var termo = nome.Trim();
        var query =
            from veiculo in db.Veiculos.AsNoTracking()
            join fabricante in db.Fabricantes on veiculo.FabricanteId equals fabricante.Id
            join categoria in db.CategoriasVeiculo on veiculo.CategoriaVeiculoId equals categoria.Id
            where fabricante.Nome.Contains(termo)
            orderby fabricante.Nome, veiculo.Modelo
            select new VeiculoFiltroResponse(veiculo.Id, fabricante.Nome, categoria.Nome,
                veiculo.Modelo, veiculo.Placa, veiculo.AnoFabricacao, veiculo.Quilometragem, veiculo.Ativo);

        return Ok(await query.ToListAsync(cancellationToken));
    }

    /// <summary>Filtro 2: veículos por categoria e quilometragem máxima (INNER JOIN).</summary>
    [HttpGet("veiculos-por-categoria")]
    public async Task<ActionResult<IEnumerable<VeiculoFiltroResponse>>> VeiculosPorCategoria(
        [FromQuery, Required, MinLength(2)] string nome,
        [FromQuery, Range(0, int.MaxValue)] int? quilometragemMaxima,
        CancellationToken cancellationToken)
    {
        var termo = nome.Trim();
        var query =
            from veiculo in db.Veiculos.AsNoTracking()
            join categoria in db.CategoriasVeiculo on veiculo.CategoriaVeiculoId equals categoria.Id
            join fabricante in db.Fabricantes on veiculo.FabricanteId equals fabricante.Id
            where categoria.Nome.Contains(termo)
                && (!quilometragemMaxima.HasValue || veiculo.Quilometragem <= quilometragemMaxima.Value)
            orderby categoria.Nome, veiculo.Modelo
            select new VeiculoFiltroResponse(veiculo.Id, fabricante.Nome, categoria.Nome,
                veiculo.Modelo, veiculo.Placa, veiculo.AnoFabricacao, veiculo.Quilometragem, veiculo.Ativo);

        return Ok(await query.ToListAsync(cancellationToken));
    }

    /// <summary>Filtro 3: aluguéis pelo nome ou CPF do cliente (INNER JOIN).</summary>
    [HttpGet("alugueis-por-cliente")]
    public async Task<ActionResult<IEnumerable<AluguelFiltroResponse>>> AlugueisPorCliente(
        [FromQuery, Required, MinLength(2)] string termo,
        CancellationToken cancellationToken)
    {
        var filtro = termo.Trim();
        var query =
            from aluguel in db.Alugueis.AsNoTracking()
            join cliente in db.Clientes on aluguel.ClienteId equals cliente.Id
            join veiculo in db.Veiculos on aluguel.VeiculoId equals veiculo.Id
            where cliente.Nome.Contains(filtro) || cliente.Cpf.Contains(filtro)
            orderby aluguel.DataRetirada descending
            select new AluguelFiltroResponse(aluguel.Id, cliente.Nome, cliente.Cpf,
                veiculo.Modelo, veiculo.Placa, aluguel.DataRetirada, aluguel.DataPrevistaDevolucao,
                aluguel.DataDevolucao, aluguel.ValorDiaria, aluguel.ValorTotal,
                aluguel.DataDevolucao != null ? "Devolvido" : "Em aberto");

        return Ok(await query.ToListAsync(cancellationToken));
    }

    /// <summary>Filtro 4: aluguéis que interceptam um período (INNER JOIN).</summary>
    [HttpGet("alugueis-por-periodo")]
    public async Task<ActionResult<IEnumerable<AluguelFiltroResponse>>> AlugueisPorPeriodo(
        [FromQuery] DateTime inicio,
        [FromQuery] DateTime fim,
        CancellationToken cancellationToken)
    {
        if (inicio == default || fim <= inicio)
            throw ApiException.BadRequest("Informe um período válido, com fim posterior ao início.");

        var query =
            from aluguel in db.Alugueis.AsNoTracking()
            join cliente in db.Clientes on aluguel.ClienteId equals cliente.Id
            join veiculo in db.Veiculos on aluguel.VeiculoId equals veiculo.Id
            where aluguel.DataRetirada < fim
                && (aluguel.DataDevolucao ?? aluguel.DataPrevistaDevolucao) > inicio
            orderby aluguel.DataRetirada
            select new AluguelFiltroResponse(aluguel.Id, cliente.Nome, cliente.Cpf,
                veiculo.Modelo, veiculo.Placa, aluguel.DataRetirada, aluguel.DataPrevistaDevolucao,
                aluguel.DataDevolucao, aluguel.ValorDiaria, aluguel.ValorTotal,
                aluguel.DataDevolucao != null ? "Devolvido" : "Em aberto");

        return Ok(await query.ToListAsync(cancellationToken));
    }

    /// <summary>Filtro 5: clientes sem nenhum aluguel (LEFT JOIN).</summary>
    [HttpGet("clientes-sem-aluguel")]
    public async Task<ActionResult<IEnumerable<ClienteSemAluguelResponse>>> ClientesSemAluguel(
        [FromQuery] string? nome,
        CancellationToken cancellationToken)
    {
        var termo = nome?.Trim();
        var query =
            from cliente in db.Clientes.AsNoTracking()
            join aluguel in db.Alugueis on cliente.Id equals aluguel.ClienteId into alugueisDoCliente
            from aluguel in alugueisDoCliente.DefaultIfEmpty()
            where aluguel == null && (termo == null || cliente.Nome.Contains(termo))
            orderby cliente.Nome
            select new ClienteSemAluguelResponse(cliente.Id, cliente.Nome, cliente.Cpf,
                cliente.Email, cliente.Ativo);

        return Ok(await query.ToListAsync(cancellationToken));
    }
}
