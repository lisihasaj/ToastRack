# Contributing to ToastRack

Thanks for your interest in contributing! 🎉

## Getting started

1. Fork and clone the repository.
2. Install the [.NET 10 SDK](https://dotnet.microsoft.com/download) (the library multi-targets
   net8.0/net9.0/net10.0; the newest SDK builds them all).
3. Build and test:

   ```bash
   dotnet build
   dotnet test -f net10.0   # or plain `dotnet test` if you have the .NET 8 runtime too
   ```

4. Try your changes in the demo app:

   ```bash
   dotnet run --project samples/ToastRack.Sample
   ```

## Guidelines

- **Open an issue first** for new features so we can discuss the API before you invest time.
- Keep the library **dependency-free** (only `Microsoft.AspNetCore.Components.Web`).
- Every public API needs XML docs; the build fails on missing docs and on warnings.
- Add or update tests (xUnit + bUnit) for any behavior change.
- Run `dotnet format` before committing — CI verifies formatting.
- New CSS must be themable: expose values as `--toastrack-*` custom properties with a default.

## Pull requests

- Target the `main` branch.
- Describe *what* and *why*; link the related issue.
- CI must be green (build, format check, tests on all target frameworks).

## Release process (maintainers)

Releases are tag-driven: pushing a tag `vX.Y.Z` triggers the release workflow, which packs with
[MinVer](https://github.com/adamralph/minver) and publishes to NuGet.org, then creates a GitHub Release.

## Code of Conduct

This project follows the [Contributor Covenant](CODE_OF_CONDUCT.md). Be kind.
