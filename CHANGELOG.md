# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/lisihasaj/ToastRack/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/lisihasaj/ToastRack/releases/tag/v0.1.0
