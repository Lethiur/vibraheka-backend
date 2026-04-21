# =============================================================================
# quality_gate.ps1 — Ejecuta tests con cobertura y verifica umbral del 80%
#
# Uso:
#   .\tool\quality_gate.ps1
#
# Requisitos:
#   - dotnet SDK instalado
#   - reportgenerator: dotnet tool install -g dotnet-reportgenerator-globaltool
# =============================================================================

$ErrorActionPreference = 'Stop'

$RepoRoot   = (Resolve-Path (Join-Path $PSScriptRoot '..'))
$CoverageDir = Join-Path $RepoRoot 'coverage'
$Threshold  = 80

Set-Location $RepoRoot

Write-Host ""
Write-Host "========================================"
Write-Host "  🧪  Ejecutando tests con cobertura"
Write-Host "========================================"

# Limpiar resultados anteriores
if (Test-Path $CoverageDir) {
    Remove-Item -Recurse -Force $CoverageDir
}

# Ejecutar tests con cobertura (solo proyectos Unit + Functional, no Integration/Acceptance que requieren AWS)
dotnet test `
    --collect:"XPlat Code Coverage" `
    --results-directory $CoverageDir `
    -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[VibraHeka.Application]*,[VibraHeka.Domain]*,[VibraHeka.Infrastructure]*" `
    DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*Tests]*"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌  dotnet test falló con código de salida $LASTEXITCODE"
    exit 1
}

Write-Host ""
Write-Host "========================================"
Write-Host "  📊  Generando reporte de cobertura"
Write-Host "========================================"

# Verificar que reportgenerator está instalado
$rgCmd = Get-Command reportgenerator -ErrorAction SilentlyContinue
if (-not $rgCmd) {
    Write-Host "⚠️  reportgenerator no encontrado. Instalando..."
    dotnet tool install -g dotnet-reportgenerator-globaltool
    if ($LASTEXITCODE -ne 0) {
        Write-Host "❌  No se pudo instalar reportgenerator."
        exit 1
    }
}

reportgenerator `
    "-reports:$CoverageDir\**\coverage.cobertura.xml" `
    "-targetdir:$CoverageDir\report" `
    "-reporttypes:Html;TextSummary;Cobertura" `
    "-verbosity:Warning"

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌  reportgenerator falló con código de salida $LASTEXITCODE"
    exit 1
}

Write-Host ""
Write-Host "========================================"
Write-Host "  ✅  Verificando umbral de cobertura ($Threshold%)"
Write-Host "========================================"

$SummaryFile = Join-Path $CoverageDir 'report\Summary.txt'

if (-not (Test-Path $SummaryFile)) {
    Write-Host "❌  No se encontró el fichero de resumen: $SummaryFile"
    exit 1
}

# Extraer cobertura de línea del resumen de texto
$SummaryContent = Get-Content $SummaryFile
$LineCoverageLine = $SummaryContent | Select-String -Pattern 'Line coverage:'
if (-not $LineCoverageLine) {
    Write-Host "❌  No se encontró la línea 'Line coverage:' en el resumen."
    exit 1
}

$LineCoverageStr = ($LineCoverageLine.Line -split ':')[1].Trim().TrimEnd('%').Trim()
$LineCoverage    = [double]$LineCoverageStr
$LineCoverageInt = [int][Math]::Floor($LineCoverage)

Write-Host "📈  Cobertura de línea: $LineCoverage%"

if ($LineCoverageInt -lt $Threshold) {
    Write-Host ""
    Write-Host "❌  QUALITY GATE FAILED — Cobertura $LineCoverage% < umbral $Threshold%"
    Write-Host "   Reporte HTML: $CoverageDir\report\index.html"
    exit 1
}

Write-Host ""
Write-Host "✅  QUALITY GATE PASSED — Cobertura $LineCoverage% >= umbral $Threshold%"
Write-Host "   Reporte HTML: $CoverageDir\report\index.html"

Write-Host ""
Write-Host "========================================"
Write-Host "  🎨  Verificando formato de código"
Write-Host "========================================"

dotnet format --verify-no-changes

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "❌  FORMAT CHECK FAILED — Hay ficheros con problemas de formato."
    Write-Host "   Ejecuta 'dotnet format' para corregirlos."
    exit 1
}

Write-Host "✅  FORMAT CHECK PASSED"

Write-Host ""
Write-Host "========================================"
Write-Host "  🏁  QUALITY GATE COMPLETO — TODO OK"
Write-Host "========================================"

