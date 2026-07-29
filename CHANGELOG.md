# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial release of ToastRack.
- `IToastService` / `ToastService` with success, warning, error, info, loading and custom variants.
- `ToastRackHost` component with five stacking positions and optional `BoundarySelector`
  (ResizeObserver-tracked boundary element).
- Loading toasts with indeterminate spinner or determinate progress circle, collapse/expand
  behavior for multiple loading toasts, and resolve-into-result-toast flow.
- Action buttons (`ToastAction`), hover-to-pause expiry, click-to-dismiss, deduplication by `ToastId`.
- Inline SVG icons and pure-CSS spinner — no external dependencies.
- Theming via `--toastrack-*` CSS custom properties.
- Thread-safe service suitable for Blazor Server and background workers.

[Unreleased]: https://github.com/lisihasaj/ToastRack/commits/main
