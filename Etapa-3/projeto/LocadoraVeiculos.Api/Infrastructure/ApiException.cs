namespace LocadoraVeiculos.Api.Infrastructure;

public sealed class ApiException(int statusCode, string title, string detail) : Exception(detail)
{
    public int StatusCode { get; } = statusCode;
    public string Title { get; } = title;

    public static ApiException NotFound(string recurso, int id) =>
        new(StatusCodes.Status404NotFound, "Recurso não encontrado", $"{recurso} com ID {id} não foi encontrado.");

    public static ApiException Conflict(string detail) =>
        new(StatusCodes.Status409Conflict, "Conflito com os dados existentes", detail);

    public static ApiException BadRequest(string detail) =>
        new(StatusCodes.Status400BadRequest, "Parâmetros inválidos", detail);
}
