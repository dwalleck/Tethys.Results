#!/bin/bash
# verify-documentation.sh
# Verifies that all public APIs have XML documentation

set -e

echo "📚 Verifying XML Documentation..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Ensure we treat warnings as errors for missing XML comments
echo "Building with documentation warnings as errors..."

# Create a temporary project file with stricter settings
TEMP_BUILD_PROPS=$(mktemp)
cat > "$TEMP_BUILD_PROPS" << EOF
<Project>
  <PropertyGroup>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors>CS1591</WarningsAsErrors>
    <NoWarn></NoWarn>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
EOF

# Try to build with documentation enforcement
BUILD_OUTPUT=$(dotnet build -p:CustomBeforeDirectoryBuildProps="$TEMP_BUILD_PROPS" 2>&1) || BUILD_FAILED=true

# Clean up temp file
rm -f "$TEMP_BUILD_PROPS"

if [ "$BUILD_FAILED" = "true" ]; then
    echo -e "${RED}❌ Build failed due to missing XML documentation${NC}"
    echo -e "\n${YELLOW}Missing documentation warnings:${NC}"
    echo "$BUILD_OUTPUT" | grep -E "CS1591|warning CS" | head -20
    
    # Count missing documentation warnings
    WARNING_COUNT=$(echo "$BUILD_OUTPUT" | grep -c "CS1591" || true)
    echo -e "\n${RED}Total missing documentation: $WARNING_COUNT${NC}"
    
    exit 1
fi

# Additional checks for documentation quality
echo -e "\n🔍 Checking documentation quality..."

# Check for empty XML comments
EMPTY_DOCS=$(grep -r "/// <summary></summary>" src/ --include="*.cs" || true)
if [ ! -z "$EMPTY_DOCS" ]; then
    echo -e "${YELLOW}⚠️  Found empty XML documentation:${NC}"
    echo "$EMPTY_DOCS"
fi

# Check for TODO in documentation
TODO_DOCS=$(grep -r "/// .*TODO" src/ --include="*.cs" || true)
if [ ! -z "$TODO_DOCS" ]; then
    echo -e "${YELLOW}⚠️  Found TODO in documentation:${NC}"
    echo "$TODO_DOCS"
fi

# Check README.md exists and has content
if [ ! -f "README.md" ]; then
    echo -e "${RED}❌ README.md not found${NC}"
    exit 1
fi

README_LINES=$(wc -l < README.md)
if [ "$README_LINES" -lt 50 ]; then
    echo -e "${YELLOW}⚠️  README.md seems too short (only $README_LINES lines)${NC}"
fi

# Check for examples directory
if [ ! -d "docs" ]; then
    echo -e "${YELLOW}⚠️  docs/ directory not found${NC}"
elif [ ! -f "docs/examples.md" ]; then
    echo -e "${YELLOW}⚠️  docs/examples.md not found${NC}"
fi

# Verify CHANGELOG.md is updated
if [ -f "CHANGELOG.md" ]; then
    # Check if CHANGELOG has been modified in current branch
    CHANGELOG_MODIFIED=$(git diff --name-only origin/main...HEAD | grep "CHANGELOG.md" || true)
    if [ -z "$CHANGELOG_MODIFIED" ]; then
        # Check if there are any implementation changes
        IMPL_CHANGES=$(git diff --name-only origin/main...HEAD | grep -E "src/.*\.cs$" || true)
        if [ ! -z "$IMPL_CHANGES" ]; then
            echo -e "${YELLOW}⚠️  CHANGELOG.md not updated despite implementation changes${NC}"
        fi
    fi
else
    echo -e "${YELLOW}⚠️  CHANGELOG.md not found${NC}"
fi

echo -e "\n${GREEN}✅ Documentation verification complete!${NC}"

# Summary of any warnings
if [ ! -z "$EMPTY_DOCS" ] || [ ! -z "$TODO_DOCS" ] || [ "$README_LINES" -lt 50 ]; then
    echo -e "\n${YELLOW}⚠️  Some documentation improvements recommended (see warnings above)${NC}"
fi

exit 0