# Phase 3: Performance and Quality - Agent Assignments

## ⚠️ MANDATORY: Read Before Starting
1. **READ CLAUDE.md** - Top section about AGENT-GUIDELINES.md
2. **READ AGENT-GUIDELINES.md** - Especially "Parallel Development Workflow" section
3. **FOLLOW ALL GUIDELINES** - Show command outputs, create branches, run scripts

## Agent Assignments

### Agent 1: Performance Benchmarks
**Branch**: `feature/phase3-benchmarks`
**Files to create/modify**:
- `test/Tethys.Benchmarks/Tethys.Benchmarks.csproj` (new project)
- `test/Tethys.Benchmarks/ResultBenchmarks.cs`
- `test/Tethys.Benchmarks/FunctionalMethodsBenchmarks.cs`

**Tasks**:
1. Follow AGENT-GUIDELINES.md "Parallel Development Workflow"
2. Create benchmark project using BenchmarkDotNet
3. Implement benchmarks for:
   - Result creation (success vs failure)
   - Match method performance
   - Map/FlatMap operations
   - Callback methods
   - Try pattern overhead
4. Run benchmarks and document results

### Agent 2: Code Coverage Setup
**Branch**: `feature/phase3-coverage`
**Files to modify**:
- `.github/workflows/quality-gates.yml`
- `Directory.Build.props` (if needed)
- Update all `.csproj` files for coverage

**Tasks**:
1. Follow AGENT-GUIDELINES.md "Parallel Development Workflow"
2. Set up Coverlet for code coverage
3. Update CI/CD to include coverage reports
4. Add coverage badges to README
5. Ensure coverage gates work in PR checks

### Agent 3: Performance Optimizations
**Branch**: `feature/phase3-optimization`
**Files to modify**:
- `src/Tethys.Results/Result.cs` (Combine method)
- `src/Tethys.Results/GenericResult.cs` (Combine method)
- `src/Tethys.Results/ResultExtensions.cs` (async methods)

**Tasks**:
1. Follow AGENT-GUIDELINES.md "Parallel Development Workflow"
2. Run Agent 1's benchmarks first (baseline)
3. Optimize Combine methods (avoid multiple enumerations)
4. Add ConfigureAwait(false) to all async methods
5. Re-run benchmarks to show improvements

## Coordination Rules

1. **All agents start simultaneously**
2. **All agents MUST**:
   - Show branch creation: `git checkout -b feature/phase3-XXX`
   - Create progress files: `progress/agent-N-status.md`
   - Show test/build outputs
   - Run verification scripts and show output
   - Create completion marker: `progress/READY-agent-N`

3. **Merge Order**:
   - Agent 2 first (infrastructure)
   - Agent 1 second (baseline metrics)
   - Agent 3 last (shows improvements)

## Example of Expected Output

Every agent response should look like:

```
## Agent 1: Performance Benchmarks

### Branch Setup
```bash
git checkout -b feature/phase3-benchmarks
# OUTPUT: Switched to a new branch 'feature/phase3-benchmarks'

git branch --show-current
# OUTPUT: feature/phase3-benchmarks
```

[... rest of work following AGENT-GUIDELINES.md format ...]
```

## Verification Before Merge

No agent work is complete until:
1. Branch created and shown
2. All verification scripts pass with output shown
3. Progress files exist with completion marker
4. All tests pass
5. Coverage maintained >95%