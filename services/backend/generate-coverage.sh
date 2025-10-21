#!/bin/bash

# Script to generate code coverage reports for LLM Token Price backend
# Usage: ./generate-coverage.sh [--open]
# Options:
#   --open    Open HTML report in browser after generation

set -e  # Exit on error

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo "🧪 Generating code coverage report..."
echo ""

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

# Clean previous coverage data
echo "🧹 Cleaning previous coverage data..."
rm -rf coverage-temp coverage-report

# Run tests with coverage collection
echo "🚀 Running tests with coverage collection..."
echo ""

dotnet test \
  --configuration Release \
  --collect:"XPlat Code Coverage" \
  --results-directory ./coverage-temp \
  --settings coverlet.runsettings \
  --verbosity quiet

if [ $? -ne 0 ]; then
  echo -e "${RED}❌ Tests failed! Coverage report not generated.${NC}"
  exit 1
fi

echo ""
echo "📊 Generating coverage reports..."

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
  echo -e "${YELLOW}⚠️  ReportGenerator not found. Installing...${NC}"
  dotnet tool install -g dotnet-reportgenerator-globaltool
fi

# Generate HTML and text reports
reportgenerator \
  -reports:"coverage-temp/**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:"Html;TextSummary;Badges" \
  -verbosity:Warning

echo ""
echo -e "${GREEN}✅ Coverage report generated successfully!${NC}"
echo ""

# Display summary
echo "📈 Coverage Summary:"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
cat coverage-report/Summary.txt | grep -A 20 "Summary"
echo "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━"
echo ""

# Extract key metrics
LINE_COVERAGE=$(cat coverage-report/Summary.txt | grep "Line coverage:" | awk '{print $3}')
BRANCH_COVERAGE=$(cat coverage-report/Summary.txt | grep "Branch coverage:" | awk '{print $3}')
METHOD_COVERAGE=$(cat coverage-report/Summary.txt | grep "Method coverage:" | awk '{print $3}')

echo -e "📊 Key Metrics:"
echo -e "   Line Coverage:   $LINE_COVERAGE"
echo -e "   Branch Coverage: $BRANCH_COVERAGE"
echo -e "   Method Coverage: $METHOD_COVERAGE"
echo ""

# Check thresholds
LINE_NUM=$(echo $LINE_COVERAGE | sed 's/%//')
if (( $(echo "$LINE_NUM >= 70" | bc -l) )); then
  echo -e "${GREEN}✅ Line coverage meets target (≥70%)${NC}"
else
  echo -e "${RED}❌ Line coverage below target (≥70%)${NC}"
fi

# Check domain coverage
DOMAIN_COVERAGE=$(cat coverage-report/Summary.txt | grep "LlmTokenPrice.Domain " | awk '{print $2}' | sed 's/%//')
if [ ! -z "$DOMAIN_COVERAGE" ]; then
  if (( $(echo "$DOMAIN_COVERAGE >= 90" | bc -l) )); then
    echo -e "${GREEN}✅ Domain coverage meets target (≥90%)${NC}"
  else
    echo -e "${RED}❌ Domain coverage below target (≥90%)${NC}"
  fi
fi

echo ""
echo "📁 Report location:"
echo "   HTML: file://$SCRIPT_DIR/coverage-report/index.html"
echo "   Text: $SCRIPT_DIR/coverage-report/Summary.txt"
echo ""

# Open report in browser if requested
if [[ "$1" == "--open" ]]; then
  echo "🌐 Opening coverage report in browser..."

  if command -v xdg-open &> /dev/null; then
    xdg-open "coverage-report/index.html"
  elif command -v open &> /dev/null; then
    open "coverage-report/index.html"
  else
    echo -e "${YELLOW}⚠️  Could not open browser automatically${NC}"
    echo "   Please open: coverage-report/index.html"
  fi
fi

echo "✨ Done!"
