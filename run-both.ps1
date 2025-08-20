Param(
    [switch]$Watch
)

$ErrorActionPreference = 'Stop'

# Resolve repo root (folder where this script lives)
$root = $PSScriptRoot

# Paths
$apiSln  = Join-Path $root 'AntojeriaTica_Api\AntojeriaTica_Api.sln'
$webSln  = Join-Path $root 'AntojeriaTica_Web\AntojeriaTica_Web.sln'
$apiProj = Join-Path $root 'AntojeriaTica_Api\AntojeriaTica_Api\AntojeriaTica_Api.csproj'
$webProj = Join-Path $root 'AntojeriaTica_Web\AntojeriaTica_Web\AntojeriaTica_Web.csproj'

# Basic checks
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error 'No se encontró el SDK de .NET en PATH. Instálalo o abre una consola con dotnet disponible.'
    exit 1
}

foreach ($p in @($apiSln,$webSln,$apiProj,$webProj)) {
    if (-not (Test-Path $p)) {
        Write-Error "No se encontró la ruta: $p"
        exit 1
    }
}

# Restore solutions first (opcional pero recomendado)
Write-Host 'Restaurando dependencias...' -ForegroundColor Cyan
& dotnet restore $apiSln
& dotnet restore $webSln

# Choose command: dotnet run o dotnet watch run
$baseCmd = if ($Watch) { 'dotnet watch run' } else { 'dotnet run' }

$apiCmd = "$baseCmd --project `"$apiProj`""
$webCmd = "$baseCmd --project `"$webProj`""

# Launch in two new PowerShell windows
Write-Host "Iniciando API en nueva ventana..." -ForegroundColor Green
Start-Process -FilePath 'powershell.exe' -WorkingDirectory (Split-Path $apiProj) -ArgumentList @('-NoExit','-Command', $apiCmd)

Write-Host "Iniciando Web en nueva ventana..." -ForegroundColor Green
Start-Process -FilePath 'powershell.exe' -WorkingDirectory (Split-Path $webProj) -ArgumentList @('-NoExit','-Command', $webCmd)

Write-Host "API y Web iniciadas en ventanas separadas." -ForegroundColor Yellow
