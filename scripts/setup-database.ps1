# Executar a partir de qualquer diretório: .\scripts\setup-database.ps1
# Banco de desenvolvimento: Backend/starlink.db
# Pare a API antes de executar para fazer backup consistente.
[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$backend = Join-Path $root "Backend"
$dbPath = Join-Path $backend "starlink.db"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "SDK .NET não localizado. Instale o SDK .NET 10."
}

if (Test-Path $dbPath) {
    if ((Test-Path "${dbPath}-wal") -or (Test-Path "${dbPath}-shm")) {
        throw "O banco possui arquivos WAL/SHM. Pare a API e confira o banco antes do backup."
    }
    $backupDir = Join-Path $backend "backups"
    New-Item -Path $backupDir -ItemType Directory -Force | Out-Null
    $stamp = Get-Date -Format "yyyyMMdd-HHmmssfff"
    $backup = Join-Path $backupDir "starlink-$stamp.db"
    Copy-Item -Path $dbPath -Destination $backup -ErrorAction Stop
    Write-Host "Backup local criado em: $backup"
}

Push-Location $root
try {
    & dotnet tool restore
    if ($LASTEXITCODE -ne 0) { throw "Falha ao restaurar dotnet-ef." }

    Push-Location $backend
    try {
        & dotnet tool run dotnet-ef database update --project Starlink.Api.csproj --startup-project Starlink.Api.csproj
        if ($LASTEXITCODE -ne 0) { throw "A aplicação das migrations falhou." }
    }
    finally {
        Pop-Location
    }
}
finally {
    Pop-Location
}

if (-not (Test-Path $dbPath)) {
    throw "O arquivo SQLite esperado não foi localizado em $dbPath"
}

Write-Host "Banco SQLite de desenvolvimento atualizado: $dbPath"
Write-Host "Verificação opcional: py -3 scripts/verify_database.py Backend/starlink.db"
