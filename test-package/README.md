# PhoenixLabs.Results Test Package

This is a simple test project to verify that the PhoenixLabs.Results package is working correctly after installation.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed

## Running the Test

### Using the Scripts

For convenience, we've included scripts to run the test:

- **Windows (Batch)**: Run `run-test.bat` by double-clicking it or from the command prompt
- **Windows (PowerShell)**: Run `run-test.ps1` from PowerShell with `.\run-test.ps1` (you may need to set execution policy with `Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass`)
- **macOS/Linux**: Run `run-test.sh` from the terminal (you may need to make it executable first with `chmod +x run-test.sh`)

### Manual Steps

1. Make sure the PhoenixLabs.Results package has been published to NuGet.org or your local feed.

2. If using a local feed, update the `test-package.csproj` file to point to your local feed:

```xml
<PropertyGroup>
  <RestoreSources>$(RestoreSources);C:\LocalNuGetFeed;https://api.nuget.org/v3/index.json</RestoreSources>
</PropertyGroup>
```

3. Restore the packages:

```bash
dotnet restore
```

4. Run the test:

```bash
dotnet run
```

## Expected Output

```
Testing PhoenixLabs.Results package...
Success result: Success=True, Message=Operation completed successfully
Fail result: Success=False, Message=Something went wrong
Data result: Success=True, Data=42
Fail data result: Success=False, Message=Failed to get data
Extracted value: 42
Implicit conversion: 42
All tests completed successfully!
```

## Troubleshooting

If you encounter any issues:

1. Make sure the package is available in the feed you're using.
2. Check that the package version in the .csproj file matches the version you published.
3. Try clearing the NuGet cache:

```bash
dotnet nuget locals all --clear
```

4. Rebuild the test project:

```bash
dotnet build --no-restore
```
