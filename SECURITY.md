# Security Policy

## Supported versions

Only the latest released version of ToastRack receives security fixes.

## Reporting a vulnerability

Please **do not** open a public issue for security vulnerabilities.

Instead, use GitHub's private vulnerability reporting:
**[Report a vulnerability](https://github.com/lisihasaj/ToastRack/security/advisories/new)**

You can expect an initial response within 7 days. Once a fix is available, we will publish a
patched release and credit you in the advisory (unless you prefer to stay anonymous).

## Supply-chain notes

- The package has **no runtime dependencies** beyond `Microsoft.AspNetCore.Components.Web`.
- Builds are deterministic, packages include SourceLink metadata and a `.snupkg` symbol package,
  and restore runs with `NuGetAudit` enabled (fails on known-vulnerable dependencies).
- Releases are published from GitHub Actions only — never from a developer machine.
