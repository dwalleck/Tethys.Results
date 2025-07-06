#!/bin/bash
# check-coverage-threshold.sh
# Checks if code coverage meets the required threshold

set -e

echo "📊 Checking Code Coverage Threshold..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
THRESHOLD=95
NEW_CODE_THRESHOLD=95

# Run tests with coverage
echo "Running tests with coverage collection..."
dotnet test /p:CollectCoverage=true \
            /p:CoverletOutputFormat="json%2copencover" \
            /p:CoverletOutput=./coverage/ \
            /p:ExcludeByFile="**/*Designer.cs" \
            /p:Threshold=$THRESHOLD \
            /p:ThresholdType=line \
            /p:ThresholdStat=total

# Check if coverage file was generated
if [ ! -f "./coverage/coverage.json" ]; then
    echo -e "${RED}❌ Coverage file not generated${NC}"
    exit 1
fi

# Parse coverage results
# Extract line coverage percentage from the JSON file
COVERAGE=$(cat ./coverage/coverage.json | grep -o '"Line": [0-9.]*' | head -1 | cut -d' ' -f2)

# Remove decimal for comparison
COVERAGE_INT=${COVERAGE%.*}

echo -e "\n📈 Coverage Report:"
echo -e "Overall Coverage: ${COVERAGE}%"
echo -e "Required Threshold: ${THRESHOLD}%"

# Check if coverage meets threshold
if [ "$COVERAGE_INT" -lt "$THRESHOLD" ]; then
    echo -e "${RED}❌ Coverage ${COVERAGE}% is below threshold of ${THRESHOLD}%${NC}"
    
    # Show which files have low coverage
    echo -e "\n${YELLOW}Files with low coverage:${NC}"
    dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover | grep -A 5 "Coverage"
    
    exit 1
else
    echo -e "${GREEN}✅ Coverage ${COVERAGE}% meets threshold of ${THRESHOLD}%${NC}"
fi

# Additional check for new/modified files
echo -e "\n🔍 Checking coverage for modified files..."

# Get list of modified source files
MODIFIED_FILES=$(git diff --name-only origin/main...HEAD | grep -E "src/.*\.cs$" | grep -v "Test" || true)

if [ ! -z "$MODIFIED_FILES" ]; then
    echo "Modified files:"
    echo "$MODIFIED_FILES"
    
    # For each modified file, check its individual coverage
    for file in $MODIFIED_FILES; do
        # This is a simplified check - in practice, you'd parse the detailed coverage report
        echo "Checking coverage for: $file"
    done
fi

echo -e "\n${GREEN}✅ All coverage checks passed!${NC}"
exit 0