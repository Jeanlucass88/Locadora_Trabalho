using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LocadoraVeiculos.Api.Infrastructure;

/// <summary>Documenta no OpenAPI os códigos HTTP padronizados pela API.</summary>
public sealed class StandardResponsesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant();
        var relativePath = context.ApiDescription.RelativePath ?? string.Empty;
        var success = operation.Responses.TryGetValue("200", out var documentedSuccess)
            ? documentedSuccess
            : new OpenApiResponse();

        switch (method)
        {
            case "POST":
                operation.Responses.Remove("200");
                success.Description = "Recurso criado com sucesso.";
                operation.Responses["201"] = success;
                AddProblem(operation, context, "400", "Dados de entrada inválidos.");
                if (relativePath.StartsWith("api/veiculos") || relativePath.StartsWith("api/alugueis"))
                    AddProblem(operation, context, "404", "Relacionamento informado não encontrado.");
                AddProblem(operation, context, "409", "Conflito com os dados existentes.");
                break;
            case "PUT":
                operation.Responses.Remove("200");
                operation.Responses["204"] = new OpenApiResponse { Description = "Atualização concluída." };
                AddProblem(operation, context, "400", "Dados de entrada inválidos.");
                AddProblem(operation, context, "404", "Recurso ou relacionamento não encontrado.");
                AddProblem(operation, context, "409", "Conflito com os dados existentes.");
                break;
            case "DELETE":
                operation.Responses.Remove("200");
                operation.Responses["204"] = new OpenApiResponse { Description = "Exclusão concluída." };
                AddProblem(operation, context, "404", "Recurso não encontrado.");
                if (!relativePath.StartsWith("api/alugueis"))
                    AddProblem(operation, context, "409", "Recurso possui registros dependentes.");
                break;
            case "GET":
                success.Description = "Consulta realizada com sucesso.";
                operation.Responses["200"] = success;
                if (relativePath.Contains("{id}"))
                    AddProblem(operation, context, "404", "Recurso não encontrado.");
                if (context.ApiDescription.ParameterDescriptions.Count > 0)
                    AddProblem(operation, context, "400", "Parâmetros inválidos.");
                break;
        }

        AddProblem(operation, context, "500", "Erro interno do servidor.");
    }

    private static void AddProblem(
        OpenApiOperation operation,
        OperationFilterContext context,
        string status,
        string description)
    {
        var schema = context.SchemaGenerator.GenerateSchema(typeof(ProblemDetails), context.SchemaRepository);
        operation.Responses[status] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new() { Schema = schema }
            }
        };
    }
}
