Write-Host "Running PhoenixLabs.Results test package..." -ForegroundColor Cyan
Write-Host ""

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to restore packages." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to build project." -ForegroundColor Red
    exit $LASTEXITCODE
}

# Run the test
Write-Host "Running test..." -ForegroundColor Yellow
Write-Host ""
dotnet run
if ($LASTEXITCODE -ne 0) {
    Write-Host "Test failed with error code $LASTEXITCODE." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Test completed successfully!" -ForegroundColor Green
Write-Host "Press any key to continue..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
