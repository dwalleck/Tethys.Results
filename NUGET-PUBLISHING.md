# Publishing PhoenixLabs.Results to NuGet

This guide explains how to pack and publish the PhoenixLabs.Results library to NuGet.

> **Note:** This project includes GitHub Actions workflows for automated building, testing, and publishing. See the [Automated Publishing](#automated-publishing) section below for details.

## Prerequisites

- [.NET SDK](https://dotnet.microsoft.com/download) installed
- A [NuGet account](https://www.nuget.org/users/account/LogOn) (for publishing to nuget.org)
- An API key from NuGet (for publishing to nuget.org)

## Building and Packing

1. Navigate to the project directory:

```bash
cd src/PhoenixLabs.Results
```

2. Build the project in Release mode:

```bash
dotnet build -c Release
```

3. Create the NuGet package:

```bash
dotnet pack -c Release -o ../../nupkgs
```

This will create both the main package (.nupkg) and the symbols package (.snupkg) in the `nupkgs` directory.

## Publishing to NuGet.org

1. Push the package to NuGet.org:

```bash
dotnet nuget push ../../nupkgs/PhoenixLabs.Results.1.0.0.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Replace `YOUR_API_KEY` with your NuGet API key.

2. Push the symbols package to NuGet.org:

```bash
dotnet nuget push ../../nupkgs/PhoenixLabs.Results.1.0.0.snupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

## Publishing to a Local Feed or Private Repository

If you want to publish to a local feed or private repository:

```bash
dotnet nuget push ../../nupkgs/PhoenixLabs.Results.1.0.0.nupkg --source "YOUR_LOCAL_FEED_PATH"
```

Replace `YOUR_LOCAL_FEED_PATH` with the path to your local feed or the URL of your private repository.

## Verifying the Package

After publishing, you can verify that your package is available:

1. For NuGet.org, visit https://www.nuget.org/packages/PhoenixLabs.Results
2. For a local feed, check the feed directory or use the NuGet Package Manager in Visual Studio

## Updating the Package

When you need to update the package:

1. Update the version number in the .csproj file
2. Update the CHANGELOG.md file with the changes
3. Rebuild, repack, and republish following the steps above

## Automated Publishing

This project includes GitHub Actions workflows for automated building, testing, and publishing:

### Build and Test Workflow

The `build-and-test.yml` workflow automatically builds and tests the project on every push to the main branch and pull requests. It also creates NuGet packages and uploads them as artifacts for inspection.

### Publish NuGet Package Workflow

The `publish-nuget.yml` workflow automatically publishes the package to NuGet.org when a new release is created on GitHub.

To use this workflow:

1. Ensure your repository is set up on GitHub
2. Add your NuGet API key as a GitHub secret named `NUGET_API_KEY`:
   - Go to your repository on GitHub
   - Click on "Settings" > "Secrets and variables" > "Actions"
   - Click "New repository secret"
   - Name: `NUGET_API_KEY`
   - Value: Your NuGet API key
3. Create a new release on GitHub:
   - Go to your repository on GitHub
   - Click on "Releases" > "Create a new release"
   - Tag version: v1.0.0 (should match the version in your .csproj file)
   - Release title: Version 1.0.0
   - Description: Copy the relevant section from CHANGELOG.md
   - Click "Publish release"

The workflow will automatically build, test, and publish the package to NuGet.org.
