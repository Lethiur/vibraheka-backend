#!/usr/bin/env bash
# =============================================================================
# quality_gate.sh — Ejecuta tests con cobertura y verifica umbral del 80%
#
# Uso:
#   ./tool/quality_gate.sh
#
# Requisitos:
#   - dotnet SDK instalado
#   - reportgenerator: dotnet tool install -g dotnet-reportgenerator-globaltool
# =============================================================================

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
COVERAGE_DIR="$REPO_ROOT/coverage"
THRESHOLD=80

cd "$REPO_ROOT"

echo ""
echo "========================================"
echo "  🧪  Ejecutando tests con cobertura"
echo "========================================"

# Limpiar resultados anteriores
rm -rf "$COVERAGE_DIR"

# Ejecutar tests con cobertura (solo proyectos Unit + Functional, no Integration/Acceptance que requieren AWS)
dotnet test \
  --collect:"XPlat Code Coverage" \
  --results-directory "$COVERAGE_DIR" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="[VibraHeka.Application]*,[VibraHeka.Domain]*,[VibraHeka.Infrastructure]*" \
  DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="[*Tests]*"

echo ""
echo "========================================"
echo "  📊  Generando reporte de cobertura"
echo "========================================"

# Verificar que reportgenerator está instalado
if ! command -v reportgenerator &> /dev/null; then
  echo "⚠️  reportgenerator no encontrado. Instalando..."
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

reportgenerator \
  -reports:"$COVERAGE_DIR/**/coverage.cobertura.xml" \
  -targetdir:"$COVERAGE_DIR/report" \
  -reporttypes:"Html;TextSummary;Cobertura" \
  -verbosity:Warning

echo ""
echo "========================================"
echo "  ✅  Verificando umbral de cobertura ($THRESHOLD%)"
echo "========================================"

# Extraer cobertura de línea del resumen de texto
SUMMARY_FILE="$COVERAGE_DIR/report/Summary.txt"

if [[ ! -f "$SUMMARY_FILE" ]]; then
  echo "❌  No se encontró el fichero de resumen: $SUMMARY_FILE"
  exit 1
fi

LINE_COVERAGE=$(awk -F': ' '/Line coverage:/{gsub(/%/,"",$2); print $2; exit}' "$SUMMARY_FILE")
LINE_COVERAGE=${LINE_COVERAGE:-0}
LINE_COVERAGE_INT=${LINE_COVERAGE%.*}

echo "📈  Cobertura de línea: ${LINE_COVERAGE}%"

if [[ "$LINE_COVERAGE_INT" -lt "$THRESHOLD" ]]; then
  echo ""
  echo "❌  QUALITY GATE FAILED — Cobertura ${LINE_COVERAGE}% < umbral ${THRESHOLD}%"
  echo "   Reporte HTML: $COVERAGE_DIR/report/index.html"
  exit 1
fi

echo ""
echo "✅  QUALITY GATE PASSED — Cobertura ${LINE_COVERAGE}% >= umbral ${THRESHOLD}%"
echo "   Reporte HTML: $COVERAGE_DIR/report/index.html"

echo ""
echo "========================================"
echo "  🎨  Verificando formato de código"
echo "========================================"

dotnet format --verify-no-changes
FORMAT_EXIT=$?

if [[ "$FORMAT_EXIT" -ne 0 ]]; then
  echo ""
  echo "❌  FORMAT CHECK FAILED — Hay ficheros con problemas de formato."
  echo "   Ejecuta 'dotnet format' para corregirlos."
  exit 1
fi

echo "✅  FORMAT CHECK PASSED"

echo ""
echo "========================================"
echo "  🏁  QUALITY GATE COMPLETO — TODO OK"
echo "========================================"

