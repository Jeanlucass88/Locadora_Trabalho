param(
    [string]$BaseUrl = "http://127.0.0.1:5190",
    [string]$OutputPath = "$PSScriptRoot/resultados-testes.json",
    [switch]$PreservarDados
)

$ErrorActionPreference = "Stop"
$results = [System.Collections.Generic.List[object]]::new()
$sequence = 0

function Invoke-ApiTest {
    param(
        [string]$Group,
        [string]$Method,
        [string]$Path,
        [int[]]$ExpectedStatus,
        [object]$Body = $null,
        [string]$Description = ""
    )

    $script:sequence++
    $uri = "$BaseUrl$Path"
    $parameters = @{
        Uri = $uri
        Method = $Method
        UseBasicParsing = $true
        ErrorAction = "Stop"
    }
    if ($null -ne $Body) {
        $parameters.ContentType = "application/json"
        $parameters.Body = $Body | ConvertTo-Json -Depth 8 -Compress
    }

    $status = 0
    $content = ""
    try {
        $response = Invoke-WebRequest @parameters
        $status = [int]$response.StatusCode
        $content = $response.Content
    }
    catch {
        if ($null -eq $_.Exception.Response) { throw }
        $status = [int]$_.Exception.Response.StatusCode
        $stream = $_.Exception.Response.GetResponseStream()
        if ($null -ne $stream) {
            $reader = [System.IO.StreamReader]::new($stream)
            $content = $reader.ReadToEnd()
            $reader.Dispose()
        }
    }

    $passed = $ExpectedStatus -contains $status
    $compactContent = if ($content.Length -gt 900) { $content.Substring(0, 900) + "..." } else { $content }
    $result = [pscustomobject]@{
        ordem = $script:sequence
        grupo = $Group
        descricao = $Description
        metodo = $Method
        rota = $Path
        statusEsperado = ($ExpectedStatus -join " ou ")
        statusObtido = $status
        aprovado = $passed
        corpo = $compactContent
    }
    $results.Add($result)
    $marker = if ($passed) { "OK" } else { "FALHOU" }
    Write-Host ("[{0}] {1,-6} {2,-62} HTTP {3}" -f $marker, $Method, $Path, $status)
    if (-not $passed) { throw "Teste reprovado: $Method $Path retornou $status." }
    return $content
}

# Criações usadas pelos demais testes.
$manufacturerJson = Invoke-ApiTest "Fabricantes" "POST" "/api/fabricantes" @(201) @{
    nome = "Volkswagen"; paisOrigem = "Alemanha"
} "Criar fabricante"
$manufacturerId = ($manufacturerJson | ConvertFrom-Json).id

$categoryJson = Invoke-ApiTest "Categorias" "POST" "/api/categorias-veiculo" @(201) @{
    nome = "SUV"; descricao = "Veículo utilitário esportivo"
} "Criar categoria"
$categoryId = ($categoryJson | ConvertFrom-Json).id

$clientJson = Invoke-ApiTest "Clientes" "POST" "/api/clientes" @(201) @{
    nome = "Maria da Silva"; cpf = "52998224725"; email = "maria@example.com"
    telefone = "31999999999"; dataNascimento = "1995-05-10"
    numeroCnh = "12345678901"; validadeCnh = "2030-12-31"; ativo = $true
} "Criar cliente"
$clientId = ($clientJson | ConvertFrom-Json).id

$secondClientJson = Invoke-ApiTest "Preparação dos filtros" "POST" "/api/clientes" @(201) @{
    nome = "João sem Locação"; cpf = "11144477735"; email = "joao@example.com"
    telefone = "31988888888"; dataNascimento = "1990-03-15"
    numeroCnh = "10987654321"; validadeCnh = "2031-06-30"; ativo = $true
} "Criar cliente sem aluguel para comprovar o LEFT JOIN"
$secondClientId = ($secondClientJson | ConvertFrom-Json).id

$vehicleJson = Invoke-ApiTest "Veículos" "POST" "/api/veiculos" @(201) @{
    fabricanteId = $manufacturerId; categoriaVeiculoId = $categoryId; modelo = "T-Cross"
    placa = "BRA2E19"; chassi = "9BWZZZ377VT004251"; anoFabricacao = 2024
    quilometragem = 12000; ativo = $true
} "Criar veículo"
$vehicleId = ($vehicleJson | ConvertFrom-Json).id

$rentalJson = Invoke-ApiTest "Aluguéis" "POST" "/api/alugueis" @(201) @{
    clienteId = $clientId; veiculoId = $vehicleId
    dataRetirada = "2026-10-01T09:00:00"; dataPrevistaDevolucao = "2026-10-05T09:00:00"
    quilometragemInicial = 12000; valorDiaria = 180.00
} "Criar aluguel"
$rentalId = ($rentalJson | ConvertFrom-Json).id

# Leituras de coleção e por ID das cinco entidades.
Invoke-ApiTest "Fabricantes" "GET" "/api/fabricantes" @(200) $null "Listar fabricantes" | Out-Null
Invoke-ApiTest "Fabricantes" "GET" "/api/fabricantes/$manufacturerId" @(200) $null "Consultar fabricante por ID" | Out-Null
Invoke-ApiTest "Categorias" "GET" "/api/categorias-veiculo" @(200) $null "Listar categorias" | Out-Null
Invoke-ApiTest "Categorias" "GET" "/api/categorias-veiculo/$categoryId" @(200) $null "Consultar categoria por ID" | Out-Null
Invoke-ApiTest "Clientes" "GET" "/api/clientes" @(200) $null "Listar clientes" | Out-Null
Invoke-ApiTest "Clientes" "GET" "/api/clientes/$clientId" @(200) $null "Consultar cliente por ID" | Out-Null
Invoke-ApiTest "Veículos" "GET" "/api/veiculos" @(200) $null "Listar veículos" | Out-Null
Invoke-ApiTest "Veículos" "GET" "/api/veiculos/$vehicleId" @(200) $null "Consultar veículo por ID" | Out-Null
Invoke-ApiTest "Aluguéis" "GET" "/api/alugueis" @(200) $null "Listar aluguéis" | Out-Null
Invoke-ApiTest "Aluguéis" "GET" "/api/alugueis/$rentalId" @(200) $null "Consultar aluguel por ID" | Out-Null

# Atualizações das cinco entidades.
Invoke-ApiTest "Fabricantes" "PUT" "/api/fabricantes/$manufacturerId" @(204) @{
    nome = "Volkswagen Brasil"; paisOrigem = "Alemanha"
} "Atualizar fabricante" | Out-Null
Invoke-ApiTest "Categorias" "PUT" "/api/categorias-veiculo/$categoryId" @(204) @{
    nome = "SUV Premium"; descricao = "SUV atualizado"
} "Atualizar categoria" | Out-Null
Invoke-ApiTest "Clientes" "PUT" "/api/clientes/$clientId" @(204) @{
    nome = "Maria Souza"; cpf = "52998224725"; email = "maria.souza@example.com"
    telefone = "31999999999"; dataNascimento = "1995-05-10"
    numeroCnh = "12345678901"; validadeCnh = "2030-12-31"; ativo = $true
} "Atualizar cliente" | Out-Null
Invoke-ApiTest "Veículos" "PUT" "/api/veiculos/$vehicleId" @(204) @{
    fabricanteId = $manufacturerId; categoriaVeiculoId = $categoryId; modelo = "T-Cross Highline"
    placa = "BRA2E19"; chassi = "9BWZZZ377VT004251"; anoFabricacao = 2024
    quilometragem = 12000; ativo = $true
} "Atualizar veículo" | Out-Null
Invoke-ApiTest "Aluguéis" "PUT" "/api/alugueis/$rentalId" @(204) @{
    clienteId = $clientId; veiculoId = $vehicleId
    dataRetirada = "2026-10-01T09:00:00"; dataPrevistaDevolucao = "2026-10-05T09:00:00"
    dataDevolucao = "2026-10-05T08:30:00"; quilometragemInicial = 12000
    quilometragemFinal = 12450; valorDiaria = 180.00; valorTotal = 720.00
} "Registrar devolução/atualizar aluguel" | Out-Null

# Cinco filtros e os dois tipos de JOIN.
Invoke-ApiTest "Filtros" "GET" "/api/filtros/veiculos-por-fabricante?nome=Volkswagen" @(200) $null "Filtro 1 - INNER JOIN" | Out-Null
Invoke-ApiTest "Filtros" "GET" "/api/filtros/veiculos-por-categoria?nome=SUV&quilometragemMaxima=50000" @(200) $null "Filtro 2 - INNER JOIN" | Out-Null
Invoke-ApiTest "Filtros" "GET" "/api/filtros/alugueis-por-cliente?termo=Maria" @(200) $null "Filtro 3 - INNER JOIN" | Out-Null
Invoke-ApiTest "Filtros" "GET" "/api/filtros/alugueis-por-periodo?inicio=2026-10-01T00%3A00%3A00&fim=2026-10-10T00%3A00%3A00" @(200) $null "Filtro 4 - INNER JOIN" | Out-Null
Invoke-ApiTest "Filtros" "GET" "/api/filtros/clientes-sem-aluguel" @(200) $null "Filtro 5 - LEFT JOIN" | Out-Null

# Tratamento de erros documentado.
Invoke-ApiTest "Validações" "POST" "/api/clientes" @(400) @{} "Entrada inválida deve retornar 400" | Out-Null
Invoke-ApiTest "Validações" "GET" "/api/fabricantes/99999" @(404) $null "Recurso inexistente deve retornar 404" | Out-Null
Invoke-ApiTest "Validações" "POST" "/api/fabricantes" @(409) @{
    nome = "Volkswagen Brasil"; paisOrigem = "Alemanha"
} "Duplicidade deve retornar 409" | Out-Null

# Exclusões em ordem inversa para respeitar as chaves estrangeiras.
if (-not $PreservarDados) {
    Invoke-ApiTest "Aluguéis" "DELETE" "/api/alugueis/$rentalId" @(204) $null "Excluir aluguel" | Out-Null
    Invoke-ApiTest "Veículos" "DELETE" "/api/veiculos/$vehicleId" @(204) $null "Excluir veículo" | Out-Null
    Invoke-ApiTest "Clientes" "DELETE" "/api/clientes/$secondClientId" @(204) $null "Excluir cliente auxiliar" | Out-Null
    Invoke-ApiTest "Clientes" "DELETE" "/api/clientes/$clientId" @(204) $null "Excluir cliente" | Out-Null
    Invoke-ApiTest "Categorias" "DELETE" "/api/categorias-veiculo/$categoryId" @(204) $null "Excluir categoria" | Out-Null
    Invoke-ApiTest "Fabricantes" "DELETE" "/api/fabricantes/$manufacturerId" @(204) $null "Excluir fabricante" | Out-Null
}

$outputDirectory = Split-Path -Parent $OutputPath
if (-not (Test-Path -LiteralPath $outputDirectory)) {
    New-Item -ItemType Directory -Path $outputDirectory | Out-Null
}
$results | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath $OutputPath -Encoding UTF8

$passedCount = @($results | Where-Object aprovado).Count
Write-Host ""
Write-Host "Resultado: $passedCount/$($results.Count) chamadas aprovadas."
Write-Host "Arquivo: $OutputPath"
if ($passedCount -ne $results.Count) { exit 1 }
