# PowerShell script to remove BOM characters from files
# Usage: .\remove-bom.ps1 -Path "path\to\file.cs" or .\remove-bom.ps1 -Directory "path\to\directory" -Extension ".cs"

param (
    [string]$Path,
    [string]$Directory,
    [string]$Extension
)

function Remove-BomFromFile {
    param (
        [string]$FilePath
    )
    
    try {
        # Read the file content as bytes
        $bytes = [System.IO.File]::ReadAllBytes($FilePath)
        
        # Check if the file has a BOM
        $hasBom = $false
        if ($bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF) {
            $hasBom = $true
        }
        
        if ($hasBom) {
            Write-Host "Removing BOM from $FilePath"
            
            # Read the file content as bytes (skipping the first 3 bytes which are the BOM)
            $contentBytes = [System.IO.File]::ReadAllBytes($FilePath)
            $contentWithoutBom = $contentBytes[3..($contentBytes.Length-1)]
            
            # Write the content back without the BOM
            [System.IO.File]::WriteAllBytes($FilePath, $contentWithoutBom)
            
            Write-Host "BOM removed successfully from $FilePath"
        } else {
            Write-Host "No BOM found in $FilePath"
        }
    } catch {
        Write-Error "Error processing file $FilePath : $_"
    }
}

# Process a single file if Path is provided
if ($Path) {
    if (Test-Path $Path -PathType Leaf) {
        Remove-BomFromFile -FilePath $Path
    } else {
        Write-Error "File not found: $Path"
    }
}

# Process all files in a directory with the specified extension
if ($Directory) {
    if (Test-Path $Directory -PathType Container) {
        $files = Get-ChildItem -Path $Directory -Recurse -File | Where-Object { $_.Extension -eq $Extension }
        
        foreach ($file in $files) {
            Remove-BomFromFile -FilePath $file.FullName
        }
        
        Write-Host "Processed $($files.Count) files with extension $Extension in $Directory"
    } else {
        Write-Error "Directory not found: $Directory"
    }
}

# If neither Path nor Directory is provided, show usage
if (-not $Path -and -not $Directory) {
    Write-Host "Usage:"
    Write-Host "  .\remove-bom.ps1 -Path 'path\to\file.cs'"
    Write-Host "  .\remove-bom.ps1 -Directory 'path\to\directory' -Extension '.cs'"
}
