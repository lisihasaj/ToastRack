# 🍞 ToastRack

[![CI](https://github.com/lisihasaj/ToastRack/actions/workflows/ci.yml/badge.svg)](https://github.com/lisihasaj/ToastRack/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/ToastRack.svg)](https://www.nuget.org/packages/ToastRack)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

A lightweight, **dependency-free** toast notification library for Blazor — WebAssembly and Server.

- ✅ Success / warning / error / info variants with built-in inline SVG icons (no icon font needed)
- ⏳ Loading toasts with indeterminate spinner **or determinate progress circle**, resolved into a regular toast when the work finishes
- 🗂 Multiple loading toasts collapse into a single "Processing…" pill (hover or click to expand)
- 🔘 Action buttons (undo / retry / …) — toasts with actions never auto-expire
- ⏸ Hover to pause the expiry timer
- 📍 Five stacking positions, plus an optional **boundary element**: anchor toasts to your content area (e.g. next to a sidebar) instead of the viewport, tracked automatically with a `ResizeObserver`
- 🎨 Fully themable via `--toastrack-*` CSS custom properties
- 🧩 Custom icon or fully custom toast content via `RenderFragment`
- ♿ `role="status"` / `role="alert"`, progressbar ARIA attributes, `prefers-reduced-motion` support

## Install

```bash
dotnet add package ToastRack
```

Supports .NET 8, 9 and 10.

## Quick start

**1. Register the service** in `Program.cs`:

```csharp
builder.Services.AddToastRack();
```

**2. Place the component** once in your layout (e.g. `MainLayout.razor`):

```razor
@using ToastRack.Components

<ToastRack />
```

ToastRack's styles ship as scoped CSS, so make sure your `index.html` / `App.razor` links the CSS bundle
(present by default in the Blazor templates):

```html
<link href="YourApp.styles.css" rel="stylesheet" />
```

**3. Show toasts** from anywhere:

```csharp
@inject IToastService Toasts

Toasts.Success("Saved", "Your changes were saved successfully.");
Toasts.Error("Upload failed");
Toasts.Info("Heads up", position: ToastPosition.TopRight);
```

Need actions, custom icons, expiry control or dedupe ids? Use the options-based methods:

```csharp
Toasts.ShowSuccessToast(new ToastOptions
{
    Title = "Saved",
    Caption = "Your changes were saved successfully.",
    Expiry = 10,
});
```

## Recipes

### Positions

```csharp
// TopLeft, TopRight, BottomLeft, BottomRight, BottomCenter
Toasts.Info("Top right", position: ToastPosition.TopRight);
```

### Action buttons

Toasts with actions never auto-expire; invoking an action closes the toast.

```csharp
Toasts.ShowInfoToast(new ToastOptions
{
    Title = "Item deleted",
    Actions =
    [
        new ToastAction { Label = "Undo", OnClick = RestoreItem },
        new ToastAction { Label = "Dismiss", Style = ToastActionStyle.Text },
    ],
});
```

### Loading toast with progress

```csharp
Toasts.Loading("upload", "Uploading...", showProgress: true);

// as the work progresses:
Toasts.Progress("upload", 60);

// when done:
Toasts.Resolve("upload", ToastVariant.Success, "Uploaded"); // or Error / Warning / Info
```

### Custom icon or fully custom content

```csharp
// custom icon
Toasts.ShowSuccessToast(new ToastOptions { Title = "Done", Icon = @<span>🎉</span> });

// fully custom toast body (the shell only provides shadow, radius and stacking)
Toasts.ShowToast(new ToastOptions
{
    Variant = ToastVariant.Custom,
    Fragment = @<div class="my-toast">Hello!</div>,
});
```

### Anchor toasts to a content area instead of the viewport

If your app has a fixed sidebar or header, anchor toasts to the main content element.
The element is tracked with a `ResizeObserver` — no manual wiring needed:

```razor
<ToastRack BoundarySelector="main" />
```

### Deduplication and programmatic removal

```csharp
// A ToastId makes the toast unique — repeated calls with the same id are ignored.
Toasts.ShowErrorToast(new ToastOptions { ToastId = "api-down", Title = "API unavailable" });

// remove it later:
Toasts.RemoveToast("api-down");
```

## Theming

Every color, size and font is a CSS custom property with a sensible default. Override any of them
in your app's CSS (e.g. on `:root`):

```css
:root {
  --toastrack-success-bg: #e8f5e9;
  --toastrack-success-accent: #2e7d32;
  --toastrack-radius: 12px;
  --toastrack-z-index: 3000;
  --toastrack-font-family: "Inter", sans-serif;
}
```

| Property | Default | Purpose |
|---|---|---|
| `--toastrack-z-index` | `2000` | Stacking order of toast containers |
| `--toastrack-radius` | `0.5rem` | Toast corner radius |
| `--toastrack-shadow` | soft two-layer shadow | Toast box shadow |
| `--toastrack-padding` | `1rem` | Toast padding |
| `--toastrack-gap` | `0.5rem` | Gap between icon / content / close |
| `--toastrack-stack-gap` | `0.5rem` | Gap between stacked toasts |
| `--toastrack-edge-gap` | `0.5rem` | Distance from the anchoring edge |
| `--toastrack-transition` | `0.2s ease` | Transition timing |
| `--toastrack-animation-duration` | `0.25s` | Entry animation duration |
| `--toastrack-font-family` | `inherit` | Toast font family |
| `--toastrack-font-size` | `0.875rem` | Title/caption font size |
| `--toastrack-title-weight` / `--toastrack-caption-weight` | `500` / `400` | Font weights |
| `--toastrack-title-color` / `--toastrack-caption-color` | `#111827` / `#4b5563` | Text colors |
| `--toastrack-icon-size` | `1.25rem` | Variant icon size |
| `--toastrack-accent-width` | `4px` | Width of the left accent bar |
| `--toastrack-success-bg` / `--toastrack-success-accent` | `#ecfdf5` / `#059669` | Success colors |
| `--toastrack-warning-bg` / `--toastrack-warning-accent` | `#fff7ed` / `#f97316` | Warning colors |
| `--toastrack-error-bg` / `--toastrack-error-accent` | `#fef2f2` / `#ef4444` | Error colors |
| `--toastrack-info-bg` / `--toastrack-info-accent` / `--toastrack-info-border` | `#ffffff` / `#3b82f6` / `#e5e7eb` | Info colors |
| `--toastrack-loading-bg` | `#eff6ff` | Loading toast background |
| `--toastrack-collapsed-bg` / `--toastrack-collapsed-color` | `#374151` / `#ffffff` | Collapsed loading pill colors |
| `--toastrack-progress-fill` / `--toastrack-progress-track` | `#3b82f6` / `#dbeafe` | Progress circle / spinner colors |
| `--toastrack-action-bg` / `--toastrack-action-color` / `--toastrack-action-border` | `#1f2937` / `#ffffff` / `#9ca3af` | Action button colors |

## API overview

`IToastService` (registered by `AddToastRack()`, scoped):

| Member | Description |
|---|---|
| `Success` / `Warning` / `Error` / `Info` `(title, caption?, position?)` | One-line shorthands |
| `Loading(id, title, caption?, showProgress?)` / `Progress(id, percentage)` / `Resolve(id, variant?, title?, caption?)` | Loading-toast shorthands |
| `ShowToast(ToastOptions)` | Show a toast (default variant `Success`) |
| `ShowSuccessToast` / `ShowWarningToast` / `ShowErrorToast` / `ShowInfoToast` | Variant shorthands |
| `ShowLoadingToast(LoadingToastOptions)` | Top-center loading toast, never auto-expires |
| `UpdateLoadingToastProgress(ToastProgressUpdate)` | Update a progress loading toast (5–100 %) |
| `ResolveLoadingToast(ResolveToastOptions)` | Replace a loading toast with a result toast |
| `RemoveToast(string)` / `RemoveToast(ToastItem)` | Remove programmatically |
| `Toasts` / `LoadingToasts` / `ToastsByPosition` | Current state (read-only snapshots) |
| `ToastsUpdated` | Raised on every change |

Behavior notes:

- Error toasts and toasts with actions never auto-expire; everything else defaults to 5 seconds (`Expiry` overrides it).
- Hovering a toast pauses its expiry timer (except error/loading toasts, which don't expire).
- The service is thread-safe, so background work can show and update toasts (in Blazor Server too).

## Sample

A full demo lives in [`samples/ToastRack.Sample`](samples/ToastRack.Sample):

```bash
dotnet run --project samples/ToastRack.Sample
```

## Development

```bash
dotnet build          # builds net8.0 / net9.0 / net10.0
dotnet test           # runs the xUnit + bUnit suite (needs .NET 8 + 10 runtimes;
                      #   use `dotnet test -f net10.0` if you only have .NET 10)
dotnet pack src/ToastRack -o artifacts
```

Versioning is tag-driven via [MinVer](https://github.com/adamralph/minver): pushing a tag `v1.2.3`
produces package version `1.2.3`. Releases are published to NuGet.org by the
[release workflow](.github/workflows/release.yml).

## Roadmap

- Localization of default resolve titles/captions
- Optional global defaults (default expiry, default position) via `AddToastRack(options => ...)`

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Security issues: see [SECURITY.md](SECURITY.md).

## License

[MIT](LICENSE)
