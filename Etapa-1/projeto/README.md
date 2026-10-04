# Locadora de Veículos — Etapa 1

Modelagem do banco de dados implementada em C#/.NET 8 com Entity Framework Core e SQL Server Express.

## Entidades

- Fabricante;
- CategoriaVeiculo;
- Veículo;
- Cliente;
- Aluguel.

## Relacionamentos

- Fabricante 1:N Veículo;
- CategoriaVeiculo 1:N Veículo;
- Cliente 1:N Aluguel;
- Veículo 1:N Aluguel.

As configurações Fluent API estão em `LocadoraVeiculos.Api/Data/Configurations`, a migração inicial em `LocadoraVeiculos.Api/Data/Migrations` e o DDL em `esquema-locadora.sql`.

## Criar o banco

```powershell
dotnet tool restore
dotnet restore
dotnet build
dotnet tool run dotnet-ef database update --project LocadoraVeiculos.Api
```
