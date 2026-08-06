# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.4.2] - 2026-08-06

### Fixed

- The title on nuget.org no longer shows the raw `<img>` tag as literal text. nuget.org strips raw
  HTML from READMEs, so the logo markup added in 0.4.1 was rendered as text rather than an image —
  an absolute image URL alone was not sufficient. The package now ships its own README variant with
  a plain-Markdown heading; the package icon continues to appear beside the title via `PackageIcon`.

### Changed

- The package README is now generated from `README.nuget.md` rather than the repository `README.md`.
  The packaged variant drops repo-relative links (which do not resolve on nuget.org) in favor of
  absolute URLs, and omits the Development, Roadmap and Contributing sections, which are only
  relevant to contributors. The repository README keeps its logo and is unchanged for GitHub readers.

## [0.4.1] - 2026-08-06

### Changed

- The package icon is now `toast.png`, replacing the previous `icon.png`.
- `PackageProjectUrl` now points at the documentation site, https://lisihasaj.github.io/ToastRack/,
  instead of the GitHub repository. The repository is still linked via `RepositoryUrl`.
- The README logo uses an absolute image URL so it renders on nuget.org, which does not resolve
  repository-relative paths.

## [0.4.0] - 2026-08-03

### Added

- `<ToastRack />` gained optional `Position`, `CloseByClick` and `Expiry` parameters that set the
  defaults for every toast. Individual `ToastOptions` still take precedence, so a toast only falls
  back to the rack for the properties it leaves unset.
- `IToastService.Defaults` and `IToastService.SetDefaults(ToastDefaults)`, plus the new
  `ToastDefaults` model. The `ToastRack` component calls `SetDefaults` from its parameters; there is
  normally no need to call it directly.

### Changed

- **Breaking:** `ToastOptions.Position` and `ToastOptions.CloseByClick` are now nullable
  (`ToastPosition?` / `bool?`) and no longer carry their own defaults. Code that *reads* these
  properties must handle `null`; code that only assigns them is unaffected.
- **Breaking:** `LoadingToastOptions.Position` is now nullable for the same reason.
- The `position` parameter of the `Success` / `Warning` / `Error` / `Info` / `Loading` shorthands is
  now `ToastPosition?` defaulting to `null` instead of `ToastPosition.BottomLeft`. Calls that pass a
  position explicitly are unaffected; calls that omit it now follow the rack default.
- Toasts capture the defaults in force when they are shown, so changing the rack's parameters
  affects subsequent toasts rather than those already on screen.

## [0.3.0] - 2026-08-01

### Added

- New `ToastPosition.TopCenter`: every toast variant can now be shown horizontally centered
  at the top, in viewport and boundary mode alike.
- `LoadingToastOptions.Position` and a `position` parameter on the `Loading(...)` shorthand:
  loading toasts can be shown at any of the six positions (default `BottomLeft`).
- `Resolve(...)` shorthand gained an optional `position` parameter.
- `--toastrack-shadow-gap` CSS property (default `1rem`) controlling the inset kept around the
  toast stack so shadows are not clipped.

### Changed

- **Breaking:** loading toasts are no longer privileged. They stack at their position in
  insertion order alongside every other variant instead of being pinned to a dedicated
  top-center container, and they no longer collapse into a "Processing…" pill when several
  are active — each loading toast stays visible individually.
- **Breaking:** `ToastVariant.LoadingCollapsed` and the `CollapsedLoadingTitle` parameter of
  `<ToastRack />` were removed, along with the `--toastrack-collapsed-bg` /
  `--toastrack-collapsed-color` CSS properties.
- **Breaking:** `IToastService.Toasts` now includes loading toasts; `LoadingToasts` remains
  as a filtered view of it.
- `ResolveToastOptions.Position` is now nullable and defaults to `null`, meaning the result
  toast keeps the position of the loading toast it replaces.
- Loading toasts now render their `Caption` (previously only the title was shown).

### Fixed

- Toast shadows are no longer clipped by the scrolling container. The rack keeps an inset on
  every side that does not carry an edge gap, and adds it on top of the edge gap where one is
  present, so the distance from the anchoring edge is unchanged.

## [0.2.0] - 2026-08-01

### Changed

- Error toasts now follow the same lifecycle as the other non-loading variants: they
  auto-expire (5-second default, `Expiry` overrides it) and pause on hover. Previously
  they never auto-expired. Only loading toasts and toasts with actions remain persistent.

## [0.1.0] - 2026-07-30

### Added

- Initial release of ToastRack.
- `IToastService` / `ToastService` with success, warning, error, info, loading and custom variants.
- `<ToastRack />` component with five stacking positions and optional `BoundarySelector`
  (ResizeObserver-tracked boundary element).
- One-line shorthands on `IToastService`: `Success` / `Warning` / `Error` / `Info`,
  plus `Loading` / `Progress` / `Resolve` for loading toasts.
- Loading toasts with indeterminate spinner or determinate progress circle, collapse/expand
  behavior for multiple loading toasts, and resolve-into-result-toast flow.
- Action buttons (`ToastAction`), hover-to-pause expiry, click-to-dismiss, deduplication by `ToastId`.
- Inline SVG icons and pure-CSS spinner — no external dependencies.
- Theming via `--toastrack-*` CSS custom properties.
- Thread-safe service suitable for Blazor Server and background workers.

[Unreleased]: https://github.com/lisihasaj/ToastRack/compare/v0.4.2...HEAD
[0.4.2]: https://github.com/lisihasaj/ToastRack/compare/v0.4.1...v0.4.2
[0.4.1]: https://github.com/lisihasaj/ToastRack/compare/v0.4.0...v0.4.1
[0.4.0]: https://github.com/lisihasaj/ToastRack/compare/v0.3.0...v0.4.0
[0.3.0]: https://github.com/lisihasaj/ToastRack/compare/v0.2.0...v0.3.0
[0.2.0]: https://github.com/lisihasaj/ToastRack/compare/v0.1.0...v0.2.0
[0.1.0]: https://github.com/lisihasaj/ToastRack/releases/tag/v0.1.0
