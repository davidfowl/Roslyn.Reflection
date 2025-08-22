# Roslyn.Reflection
Roslyn.Reflection is a .NET library that provides familiar reflection APIs over Roslyn's symbol APIs. Use it to explore the type system while writing Roslyn source generators or analyzers, enabling reuse of existing .NET reflection-based code in compile-time scenarios.

Always reference these instructions first and fallback to search or bash commands only when you encounter unexpected information that does not match the info here.

## Working Effectively

### Prerequisites and Setup
- Ensure you have a full git clone (not shallow): Run `git fetch --unshallow` if Nerdbank.GitVersioning fails
- Install .NET SDK 8.0+ (main development) and .NET 6.0 (for tests/sample): Use `curl -sSL https://dot.net/v1/dotnet-install.sh | bash -s -- --version 6.0.428 --install-dir ~/.dotnet` if .NET 6 is missing

### Build, Test, and Run Commands
**NEVER CANCEL any of these operations - they complete quickly but may take longer on slower systems:**

1. **Restore packages:** 
   - `dotnet restore Roslyn.Reflection.sln` -- takes ~2 seconds. NEVER CANCEL. Set timeout to 5+ minutes.

2. **Build solution:**
   - `dotnet build Roslyn.Reflection.sln -c Release` -- takes ~5 seconds. NEVER CANCEL. Set timeout to 10+ minutes.
   - If build fails with GitVersioning errors, run `git fetch --unshallow` first

3. **Run tests:**
   - `dotnet test Roslyn.Reflection.sln -c Release --no-build` -- takes ~4 seconds. NEVER CANCEL. Set timeout to 10+ minutes.
   - 88 tests should pass successfully
   - Tests run on .NET 6.0 runtime

4. **Run sample application:**
   - `dotnet run --project Sample` -- takes ~3 seconds. NEVER CANCEL. Set timeout to 5+ minutes.
   - Should output plugin discovery and controller analysis results

5. **Create NuGet packages:**
   - `dotnet pack Roslyn.Reflection.sln -c Release --no-build` -- takes ~1 second. NEVER CANCEL. Set timeout to 5+ minutes.

### Validation and Quality Checks
- **Code formatting:** `dotnet format --verify-no-changes` -- verifies code follows formatting standards
- **Complete build workflow:** `dotnet restore && dotnet build -c Release && dotnet test -c Release --no-build && dotnet run --project Sample`

## Validation Scenarios
Always test these scenarios after making changes:

1. **Basic functionality test:** Run the sample application and verify it outputs:
   - Plugins section showing Plugin1 and Plugin2
   - Controllers section showing MyController and AuthController with methods
   - Types with authorize attribute showing AuthController
   - Source location information

2. **Unit test coverage:** Run the full test suite and ensure all 88 tests pass

3. **Package creation:** Verify NuGet packages can be created successfully

## Project Structure

### Key Projects
- **`Roslyn.Reflection/`** - Main library (netstandard2.0) - core reflection wrappers over Roslyn APIs
- **`Roslyn.Reflection.Tests/`** - Unit tests (net6.0) - comprehensive test coverage with xUnit  
- **`Sample/`** - Sample application (net6.0) - demonstrates finding plugins and controllers using reflection APIs

### Important Files
- **`Roslyn.Reflection.sln`** - Main solution file containing all projects
- **`version.json`** - Nerdbank.GitVersioning configuration for automatic versioning
- **`Directory.Build.props`** - Shared MSBuild properties and NuGet package metadata
- **`nuget.config`** - NuGet package source configuration
- **`.github/workflows/ci.yaml`** - CI pipeline that builds, tests, and publishes packages

### Key Source Files in Roslyn.Reflection/
- **`MetadataLoadContext.cs`** - Main entry point for loading Roslyn compilations as reflection contexts
- **`RoslynType.cs`** - Type wrapper implementing System.Type over ITypeSymbol
- **`RoslynMethodInfo.cs`** - Method wrapper implementing MethodInfo over IMethodSymbol
- **`RoslynPropertyInfo.cs`** - Property wrapper implementing PropertyInfo over IPropertySymbol
- **`SharedUtilities.cs`** - Internal utilities for binding flags, custom attributes, and symbol matching

## Common Tasks

### Debugging Build Issues
- **GitVersioning errors:** Run `git fetch --unshallow` to get full git history
- **Missing .NET runtime:** Install .NET 6.0 using the dotnet-install script for test execution
- **Package restore issues:** Delete `bin/` and `obj/` folders, then run `dotnet restore`

### Making Changes
1. **Always build and test locally:** Run full build workflow before committing
2. **Verify sample still works:** Run sample application to ensure reflection APIs work correctly  
3. **Check formatting:** Run `dotnet format --verify-no-changes` to ensure code style compliance
4. **Run complete validation:** Execute all validation scenarios to ensure no regressions

### CI/CD Pipeline
- Runs on Windows with .NET 7.0.x
- Builds in Release configuration, runs tests, creates packages
- Publishes to private NuGet feed on main branch pushes
- Uses Nerdbank.GitVersioning for automatic semantic versioning

## Expected Command Timings
All timings are approximate and may be longer on slower systems:
- `dotnet restore`: 2 seconds
- `dotnet build`: 5 seconds  
- `dotnet test`: 4 seconds
- `dotnet run --project Sample`: 3 seconds
- `dotnet pack`: 1 second
- `dotnet format --verify-no-changes`: 8 seconds
- Complete workflow from clean slate: ~10 seconds

**CRITICAL:** Always allow 5-10x these times in timeouts and NEVER CANCEL operations early.