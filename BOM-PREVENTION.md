# Preventing BOM Characters in VS Code

This guide explains how to prevent Byte Order Mark (BOM) characters from appearing in your code files when using Visual Studio Code.

## What is a BOM?

A Byte Order Mark (BOM) is a Unicode character (U+FEFF) that can appear at the beginning of a text file to indicate the encoding of the file. While BOMs can be useful in some contexts, they can cause issues in many programming scenarios, such as:

- Breaking shebang lines in shell scripts
- Causing syntax errors in some programming languages
- Creating issues with file concatenation
- Causing problems with some build tools and compilers

## VS Code Configuration

This repository includes configuration to prevent VS Code from adding BOM characters to files:

### Workspace Settings

The `.vscode/settings.json` file contains settings that configure VS Code to save files without BOM:

```json
{
    "files.encoding": "utf8",
    "files.autoGuessEncoding": false,
    "files.insertFinalNewline": true,
    "[csharp]": {
        "files.encoding": "utf8"
    },
    "[xml]": {
        "files.encoding": "utf8"
    },
    "[json]": {
        "files.encoding": "utf8"
    }
}
```

### EditorConfig

The `.editorconfig` file has been updated to include encoding settings:

```
[*]
charset = utf-8
```

This ensures that all editors that support EditorConfig (including VS Code) will use UTF-8 encoding without BOM.

## Removing BOM from Existing Files

If you already have files with BOM characters, you can use the provided scripts to remove them:

### Windows (PowerShell)

```powershell
# Remove BOM from a single file
.\remove-bom.ps1 -Path "path\to\file.cs"

# Remove BOM from all .cs files in a directory (including subdirectories)
.\remove-bom.ps1 -Directory "path\to\directory" -Extension ".cs"
```

### Linux/macOS (Bash)

```bash
# Make the script executable
chmod +x remove-bom.sh

# Remove BOM from a single file
./remove-bom.sh -f path/to/file.cs

# Remove BOM from all .cs files in a directory (including subdirectories)
./remove-bom.sh -d path/to/directory -e .cs
```

## Verifying Files

You can check if a file has a BOM using various methods:

### Windows (PowerShell)

```powershell
# Check the first few bytes of a file
Get-Content -Path "path\to\file.cs" -Encoding Byte -TotalCount 3 | ForEach-Object { "{0:X2}" -f $_ }
```

If the output is `EF BB BF`, the file has a BOM.

### Linux/macOS

```bash
# Check the first few bytes of a file
hexdump -n 3 -C path/to/file.cs
```

If the output starts with `ef bb bf`, the file has a BOM.

## Additional Information

- The BOM for UTF-8 is the byte sequence `EF BB BF`
- VS Code's default behavior can vary based on platform and settings
- Some tools and libraries may add BOMs automatically when writing files

By following this guide and using the provided configuration and scripts, you can ensure your project remains free of unwanted BOM characters.
