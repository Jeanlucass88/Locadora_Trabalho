# Documentação dos endpoints — Locadora de Veículos

## Informações gerais

- Tecnologia: ASP.NET Core 8 e Entity Framework Core 8.
- URL local padrão: `http://localhost:5076`.
- Swagger UI: `http://localhost:5076/swagger`.
- Especificação OpenAPI: `/swagger/v1/swagger.json`.
- Formato de entrada e saída: JSON (`application/json`).
- Datas: ISO 8601, por exemplo `2026-10-01T09:00:00`.
- Erros: padrão `ProblemDetails`, com `status`, `title`, `detail` e `instance`.

## Códigos HTTP utilizados

| Código | Significado |
|---|---|
| `200 OK` | Consulta executada com sucesso. |
| `201 Created` | Registro criado; a resposta contém o registro e o cabeçalho `Location`. |
| `204 No Content` | Atualização ou exclusão concluída, sem corpo de resposta. |
| `400 Bad Request` | JSON, parâmetros ou campos inválidos. |
| `404 Not Found` | Registro ou relacionamento informado não existe. |
| `409 Conflict` | Duplicidade, dependência ou regra de negócio impede a operação. |
| `500 Internal Server Error` | Erro inesperado ou indisponibilidade do banco. |

## Corpos de entrada

### FabricanteRequest

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `nome` | string | Sim | 2 a 100 caracteres; único. |
| `paisOrigem` | string | Não | Até 80 caracteres. |

### CategoriaVeiculoRequest

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `nome` | string | Sim | 2 a 50 caracteres; único. |
| `descricao` | string | Não | Até 250 caracteres. |

### ClienteRequest

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `nome` | string | Sim | 3 a 150 caracteres. |
| `cpf` | string | Sim | CPF válido e único. |
| `email` | string | Sim | E-mail válido, até 254 caracteres e único. |
| `telefone` | string | Não | Até 20 caracteres. |
| `dataNascimento` | date | Sim | Deve estar no passado. |
| `numeroCnh` | string | Sim | Exatamente 11 dígitos e único. |
| `validadeCnh` | date | Sim | Data válida. |
| `ativo` | boolean | Sim | `true` ou `false`. |

### VeiculoRequest

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `fabricanteId` | integer | Sim | ID positivo e existente. |
| `categoriaVeiculoId` | integer | Sim | ID positivo e existente. |
| `modelo` | string | Sim | 2 a 100 caracteres. |
| `placa` | string | Sim | Formato `ABC1234` ou `ABC1D23`; única. |
| `chassi` | string | Sim | 17 caracteres válidos; único. |
| `anoFabricacao` | integer | Sim | Entre 1886 e o próximo ano. |
| `quilometragem` | integer | Sim | Maior ou igual a zero. |
| `ativo` | boolean | Sim | `true` ou `false`. |

### AluguelCreateRequest

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `clienteId` | integer | Sim | Cliente existente, ativo e com CNH válida. |
| `veiculoId` | integer | Sim | Veículo existente e ativo. |
| `dataRetirada` | datetime | Sim | Data válida. |
| `dataPrevistaDevolucao` | datetime | Sim | Posterior à retirada. |
| `quilometragemInicial` | integer | Sim | Igual à quilometragem atual do veículo. |
| `valorDiaria` | decimal | Sim | Entre 0,01 e 99.999.999,99. |

O período não pode se sobrepor a outro aluguel do mesmo veículo.

### AluguelUpdateRequest

Possui todos os campos de criação e também:

| Campo | Tipo | Obrigatório | Validação |
|---|---|---|---|
| `dataDevolucao` | datetime/null | Condicional | Não pode anteceder a retirada. |
| `quilometragemFinal` | integer/null | Condicional | Não pode ser inferior à inicial ou à atual do veículo. |
| `valorTotal` | decimal/null | Condicional | Entre 0 e 9.999.999.999,99. |

Os três campos de devolução devem ser enviados juntos ou todos devem permanecer nulos.

## Endpoints de Fabricantes

| Método | Rota | Parâmetros/corpo | Sucesso | Possíveis erros |
|---|---|---|---|---|
| GET | `/api/fabricantes` | Nenhum | `200`, lista de fabricantes | `500` |
| GET | `/api/fabricantes/{id}` | `id` inteiro no caminho | `200`, fabricante | `404`, `500` |
| POST | `/api/fabricantes` | `FabricanteRequest` | `201`, fabricante criado | `400`, `409`, `500` |
| PUT | `/api/fabricantes/{id}` | `id` + `FabricanteRequest` | `204` | `400`, `404`, `409`, `500` |
| DELETE | `/api/fabricantes/{id}` | `id` inteiro no caminho | `204` | `404`, `409` se possuir veículos, `500` |

## Endpoints de Categorias

| Método | Rota | Parâmetros/corpo | Sucesso | Possíveis erros |
|---|---|---|---|---|
| GET | `/api/categorias-veiculo` | Nenhum | `200`, lista de categorias | `500` |
| GET | `/api/categorias-veiculo/{id}` | `id` inteiro no caminho | `200`, categoria | `404`, `500` |
| POST | `/api/categorias-veiculo` | `CategoriaVeiculoRequest` | `201`, categoria criada | `400`, `409`, `500` |
| PUT | `/api/categorias-veiculo/{id}` | `id` + `CategoriaVeiculoRequest` | `204` | `400`, `404`, `409`, `500` |
| DELETE | `/api/categorias-veiculo/{id}` | `id` inteiro no caminho | `204` | `404`, `409` se possuir veículos, `500` |

## Endpoints de Clientes

| Método | Rota | Parâmetros/corpo | Sucesso | Possíveis erros |
|---|---|---|---|---|
| GET | `/api/clientes` | Nenhum | `200`, lista de clientes | `500` |
| GET | `/api/clientes/{id}` | `id` inteiro no caminho | `200`, cliente | `404`, `500` |
| POST | `/api/clientes` | `ClienteRequest` | `201`, cliente criado | `400`, `409`, `500` |
| PUT | `/api/clientes/{id}` | `id` + `ClienteRequest` | `204` | `400`, `404`, `409`, `500` |
| DELETE | `/api/clientes/{id}` | `id` inteiro no caminho | `204` | `404`, `409` se possuir aluguéis, `500` |

## Endpoints de Veículos

| Método | Rota | Parâmetros/corpo | Sucesso | Possíveis erros |
|---|---|---|---|---|
| GET | `/api/veiculos` | Nenhum | `200`, lista com fabricante e categoria | `500` |
| GET | `/api/veiculos/{id}` | `id` inteiro no caminho | `200`, veículo | `404`, `500` |
| POST | `/api/veiculos` | `VeiculoRequest` | `201`, veículo criado | `400`, `404` para fabricante/categoria, `409`, `500` |
| PUT | `/api/veiculos/{id}` | `id` + `VeiculoRequest` | `204` | `400`, `404`, `409`, `500` |
| DELETE | `/api/veiculos/{id}` | `id` inteiro no caminho | `204` | `404`, `409` se possuir aluguéis, `500` |

## Endpoints de Aluguéis

| Método | Rota | Parâmetros/corpo | Sucesso | Possíveis erros |
|---|---|---|---|---|
| GET | `/api/alugueis` | Nenhum | `200`, lista com cliente e veículo | `500` |
| GET | `/api/alugueis/{id}` | `id` inteiro no caminho | `200`, aluguel | `404`, `500` |
| POST | `/api/alugueis` | `AluguelCreateRequest` | `201`, aluguel criado | `400`, `404`, `409`, `500` |
| PUT | `/api/alugueis/{id}` | `id` + `AluguelUpdateRequest` | `204` | `400`, `404`, `409`, `500` |
| DELETE | `/api/alugueis/{id}` | `id` inteiro no caminho | `204` | `404`, `500` |

O campo `situacao` da resposta é `Em aberto` ou `Devolvido`.

## Endpoints de filtros

| Método | Rota | Parâmetros de query | Retorno | Códigos |
|---|---|---|---|---|
| GET | `/api/filtros/veiculos-por-fabricante` | `nome` obrigatório, mínimo 2 caracteres | Veículos, fabricantes e categorias (`INNER JOIN`) | `200`, `400`, `500` |
| GET | `/api/filtros/veiculos-por-categoria` | `nome` obrigatório; `quilometragemMaxima` opcional e não negativa | Veículos, categorias e fabricantes (`INNER JOIN`) | `200`, `400`, `500` |
| GET | `/api/filtros/alugueis-por-cliente` | `termo` obrigatório, nome ou CPF | Aluguéis, clientes e veículos (`INNER JOIN`) | `200`, `400`, `500` |
| GET | `/api/filtros/alugueis-por-periodo` | `inicio` e `fim`; fim posterior ao início | Aluguéis que interceptam o período (`INNER JOIN`) | `200`, `400`, `500` |
| GET | `/api/filtros/clientes-sem-aluguel` | `nome` opcional | Clientes sem aluguel (`LEFT JOIN`) | `200`, `500` |

## Estruturas de saída

- `FabricanteResponse`: `id`, `nome`, `paisOrigem`.
- `CategoriaVeiculoResponse`: `id`, `nome`, `descricao`.
- `ClienteResponse`: `id`, dados pessoais, CNH e `ativo`.
- `VeiculoResponse`: IDs e nomes de fabricante/categoria, modelo, placa, chassi, ano, quilometragem e `ativo`.
- `AluguelResponse`: cliente, veículo, período, quilometragens, valores e `situacao`.

As consultas de coleção retornam arrays JSON; quando não existem registros, retornam `[]` com código `200`.
