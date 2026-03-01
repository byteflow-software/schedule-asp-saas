#!/usr/bin/env bash
set -eo pipefail

# Add common Windows tool paths to PATH
export PATH="/c/Program Files/dotnet:$HOME/.dotnet/tools:$PATH"
export PATH="C:/Program Files/dotnet:$PATH"

# Verify tools
"dotnet" --version >/dev/null 2>&1 || { echo "ERROR: dotnet not found."; exit 1; }
"dotnet-coverage" --version >/dev/null 2>&1 || { echo "ERROR: dotnet-coverage not found. Run: dotnet tool install -g dotnet-coverage"; exit 1; }
"reportgenerator" --version >/dev/null 2>&1 || { echo "ERROR: reportgenerator not found. Run: dotnet tool install -g dotnet-reportgenerator-globaltool"; exit 1; }

echo "dotnet $(dotnet --version)"

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
API_DIR="$ROOT_DIR/src/Scheduly.Api"
COVERAGE_OUT="$API_DIR/wwwroot/coverage"

echo "=== Cleaning previous coverage data ==="
rm -rf "$COVERAGE_OUT"
rm -rf "$ROOT_DIR/tests/Scheduly.UnitTests/TestResults"
rm -rf "$ROOT_DIR/tests/Scheduly.IntegrationTests/TestResults"
rm -rf "$ROOT_DIR/frontend/coverage"

mkdir -p "$COVERAGE_OUT/backend"
mkdir -p "$COVERAGE_OUT/frontend"

# ──────────────────────────────────────────────
# BACKEND COVERAGE (dotnet-coverage — profiler-based)
# ──────────────────────────────────────────────
echo ""
echo "=== Building test projects ==="
dotnet build "$ROOT_DIR/tests/Scheduly.UnitTests" --verbosity minimal
dotnet build "$ROOT_DIR/tests/Scheduly.IntegrationTests" --verbosity minimal

UNIT_RESULTS="$ROOT_DIR/tests/Scheduly.UnitTests/TestResults"
INT_RESULTS="$ROOT_DIR/tests/Scheduly.IntegrationTests/TestResults"
mkdir -p "$UNIT_RESULTS"
mkdir -p "$INT_RESULTS"

echo ""
echo "=== Running unit tests with coverage ==="
dotnet-coverage collect \
  -o "$UNIT_RESULTS/coverage.cobertura.xml" \
  -f cobertura \
  "dotnet test $ROOT_DIR/tests/Scheduly.UnitTests --no-build --verbosity minimal"

echo ""
echo "=== Running integration tests with coverage ==="
dotnet-coverage collect \
  -o "$INT_RESULTS/coverage.cobertura.xml" \
  -f cobertura \
  "dotnet test $ROOT_DIR/tests/Scheduly.IntegrationTests --no-build --verbosity minimal"

echo ""
echo "=== Generating backend HTML report ==="

# Find all coverage XML files and convert paths for Windows compatibility
COVERAGE_FILES=""
while IFS= read -r -d '' file; do
  winpath=$(echo "$file" | sed 's|^/\([a-zA-Z]\)/|\1:/|')
  if [ -z "$COVERAGE_FILES" ]; then
    COVERAGE_FILES="$winpath"
  else
    COVERAGE_FILES="$COVERAGE_FILES;$winpath"
  fi
done < <(find "$ROOT_DIR/tests" -name "coverage.cobertura.xml" -type f -print0)

COVERAGE_OUT_WIN=$(echo "$COVERAGE_OUT" | sed 's|^/\([a-zA-Z]\)/|\1:/|')

if [ -z "$COVERAGE_FILES" ]; then
  echo "WARNING: No coverage XML files found. Skipping backend report."
else
  reportgenerator \
    -reports:"$COVERAGE_FILES" \
    -targetdir:"$COVERAGE_OUT_WIN/backend" \
    -reporttypes:Html \
    -assemblyfilters:"+Scheduly.Api;+Scheduly.Application;+Scheduly.Domain;+Scheduly.Infrastructure" \
    -classfilters:"-Scheduly.Infrastructure.Persistence.Migrations.*"

  echo "Backend coverage report generated."
fi

# ──────────────────────────────────────────────
# FRONTEND COVERAGE
# ──────────────────────────────────────────────
echo ""
echo "=== Running frontend tests with coverage ==="
cd "$ROOT_DIR/frontend"
npx ng test --watch=false --coverage

echo ""
echo "=== Copying frontend coverage report ==="
if [ -d "$ROOT_DIR/frontend/coverage" ]; then
  cd "$ROOT_DIR/frontend/coverage" && cp -r . "$COVERAGE_OUT/frontend/"
  echo "Frontend coverage report generated."
else
  echo "WARNING: Frontend coverage directory not found."
fi

# ──────────────────────────────────────────────
# INDEX PAGE
# ──────────────────────────────────────────────
echo ""
echo "=== Creating coverage index page ==="
cat > "$COVERAGE_OUT/index.html" <<'INDEXEOF'
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>Scheduly — Coverage Reports</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; background: #f5f5f5; color: #333; min-height: 100vh; display: flex; align-items: center; justify-content: center; }
    .container { max-width: 500px; width: 100%; padding: 2rem; }
    h1 { font-size: 1.5rem; margin-bottom: 1.5rem; text-align: center; color: #1a1a2e; }
    .card { background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,.1); padding: 1.5rem; margin-bottom: 1rem; text-decoration: none; display: block; color: inherit; transition: box-shadow .2s; }
    .card:hover { box-shadow: 0 4px 16px rgba(0,0,0,.15); }
    .card h2 { font-size: 1.1rem; margin-bottom: .5rem; }
    .card p { color: #666; font-size: .9rem; }
    .footer { text-align: center; margin-top: 1.5rem; font-size: .8rem; color: #999; }
  </style>
</head>
<body>
  <div class="container">
    <h1>Scheduly — Coverage Reports</h1>
    <a class="card" href="./backend/index.html">
      <h2>Backend (.NET)</h2>
      <p>Unit Tests + Integration Tests — Cobertura do C# (API, Application, Domain, Infrastructure)</p>
    </a>
    <a class="card" href="./frontend/index.html">
      <h2>Frontend (Angular)</h2>
      <p>Jest Tests — Cobertura dos componentes, services, pipes, guards e interceptors</p>
    </a>
    <div class="footer">Gerado com coverlet + ReportGenerator + Jest</div>
  </div>
</body>
</html>
INDEXEOF

echo ""
echo "=== Done! ==="
echo "Start the API in Development mode and access /coverage"
echo "Credentials: Coverage__Username / Coverage__Password env vars"
