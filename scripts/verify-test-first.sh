#!/bin/bash
# verify-test-first.sh
# Verifies that test files are committed before implementation files

set -e

echo "🔍 Verifying Test-First Development Practice..."

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo -e "${RED}❌ Not in a git repository${NC}"
    exit 1
fi

# Get the list of modified C# files in the current branch compared to main
BRANCH=$(git rev-parse --abbrev-ref HEAD)
if [ "$BRANCH" = "main" ]; then
    echo -e "${YELLOW}⚠️  On main branch, skipping test-first verification${NC}"
    exit 0
fi

# Find implementation files that might need tests
IMPL_FILES=$(git diff --name-only origin/main...HEAD | grep -E "src/.*\.cs$" | grep -v "Test" || true)

if [ -z "$IMPL_FILES" ]; then
    echo -e "${GREEN}✓ No implementation files found${NC}"
    exit 0
fi

# Check each implementation file
MISSING_TESTS=()
for impl_file in $IMPL_FILES; do
    # Extract the class name from the file
    if [ -f "$impl_file" ]; then
        class_name=$(basename "$impl_file" .cs)
        
        # Look for corresponding test file
        test_pattern="test/.*${class_name}Tests\.cs"
        test_file=$(git diff --name-only origin/main...HEAD | grep -E "$test_pattern" || true)
        
        if [ -z "$test_file" ]; then
            # Check if test file exists in the repository
            existing_test=$(find test -name "${class_name}Tests.cs" 2>/dev/null || true)
            if [ -z "$existing_test" ]; then
                MISSING_TESTS+=("$impl_file")
                echo -e "${RED}❌ Missing tests for: $impl_file${NC}"
            else
                echo -e "${YELLOW}⚠️  Tests exist but not modified for: $impl_file${NC}"
            fi
        else
            # Verify test was committed before implementation
            impl_commit=$(git log --format="%H" -n 1 -- "$impl_file" | head -1)
            test_commit=$(git log --format="%H" -n 1 -- "$test_file" | head -1)
            
            if [ ! -z "$impl_commit" ] && [ ! -z "$test_commit" ]; then
                impl_date=$(git show -s --format=%ct "$impl_commit")
                test_date=$(git show -s --format=%ct "$test_commit")
                
                if [ "$impl_date" -lt "$test_date" ]; then
                    echo -e "${RED}❌ Implementation committed before tests for: $impl_file${NC}"
                    MISSING_TESTS+=("$impl_file")
                else
                    echo -e "${GREEN}✓ Test-first verified for: $impl_file${NC}"
                fi
            fi
        fi
    fi
done

# Summary
if [ ${#MISSING_TESTS[@]} -eq 0 ]; then
    echo -e "\n${GREEN}✅ All implementation files have tests written first!${NC}"
    exit 0
else
    echo -e "\n${RED}❌ Test-First Development violations found!${NC}"
    echo -e "${RED}Files missing tests or with tests written after implementation:${NC}"
    printf '%s\n' "${MISSING_TESTS[@]}"
    exit 1
fi