# ToastRack

A lightweight, **dependency-free** toast notification library for Blazor — WebAssembly and Server.

**[▶ Live interactive demo](https://lisihasaj.github.io/ToastRack/)** — try every variant, position and loading flow in the browser.

- ✅ Success / warning / error / info variants with built-in inline SVG icons (no icon font needed)
- ⏳ Loading toasts with indeterminate spinner **or determinate progress circle**, resolved into a regular toast when the work finishes — they stack at any position alongside every other variant
- 🔘 Action buttons (undo / retry / …) — toasts with actions never auto-expire
- ⏸ Hover to pause the expiry timer
- 📍 Six stacking positions (all four corners plus top/bottom center), plus an optional **boundary element**: anchor toasts to your content area (e.g. next to a sidebar) instead of the viewport, tracked automatically with a `ResizeObserver`
- ⚙️ **App-wide defaults** for position, click-to-dismiss and expiry, set once on `<ToastRack />` and overridable per toast
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
// TopLeft, TopCenter, TopRight, BottomLeft, BottomCenter, BottomRight
Toasts.Info("Top right", position: ToastPosition.TopRight);
Toasts.Loading("sync", "Syncing...", position: ToastPosition.TopCenter);
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
Toasts.Loading("upload", "Uploading...", showProgress: true, position: ToastPosition.TopRight);

// as the work progresses:
Toasts.Progress("upload", 60);

// when done — the result toast keeps the loading toast's position unless you pass one:
Toasts.Resolve("upload", ToastVariant.Success, "Uploaded"); // or Error / Warning / Info
```

Loading toasts stack at their position in insertion order like every other variant.
They never auto-expire and are dismissed by resolving or removing them.

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

### Set defaults for every toast

`<ToastRack />` takes optional `Position`, `CloseByClick` and `Expiry` parameters. Whatever you set
becomes the default for every toast in the app:

```razor
<ToastRack Position="ToastPosition.TopRight"
           CloseByClick="false"
           Expiry="8" />
```

The matching `ToastOptions` properties are nullable and always take precedence — set one and it
overrides the rack for that toast only:

```csharp
// Top right, sticky on click, 8 seconds — all inherited from the rack.
Toasts.Success("Saved", "Your changes were saved successfully.");

// Overrides all three for this toast alone.
Toasts.ShowInfoToast(new ToastOptions
{
    Title = "Just this one",
    Position = ToastPosition.BottomLeft,
    CloseByClick = true,
    Expiry = 3,
});
```

Leave a parameter off and the built-in default applies: `BottomLeft`, `CloseByClick` true, and a
5-second expiry.

> **Upgrading to 0.4.0 from 0.3.0 or earlier.** To make this inheritance possible, `ToastOptions.Position`,
> `ToastOptions.CloseByClick` and `LoadingToastOptions.Position` became nullable
> (`ToastPosition?` / `bool?`) and no longer carry their own defaults, and the `position` parameter
> of the `Success` / `Warning` / `Error` / `Info` / `Loading` shorthands is now `ToastPosition?`
> defaulting to `null`. Code that **assigns** these properties or passes a position explicitly keeps
> working unchanged and keeps the same behavior. Code that **reads** them — e.g.
> `if (options.CloseByClick)` or `switch (options.Position)` — must now handle `null`, which means
> "use the rack default". Omitting the position argument now follows the rack instead of always
> landing on `BottomLeft`.

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
| `--toastrack-content-gap` | `0.125rem` | Gap between title and caption |
| `--toastrack-actions-gap` | `0.25rem` | Gap between action buttons and above the action row |
| `--toastrack-stack-gap` | `0.5rem` | Gap between stacked toasts |
| `--toastrack-edge-gap` | `0.5rem` | Distance from the anchoring edge |
| `--toastrack-shadow-gap` | `1rem` | Inset around the toast stack that keeps shadows from being clipped by the scroll container |
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
| `--toastrack-progress-fill` / `--toastrack-progress-track` | `#3b82f6` / `#dbeafe` | Progress circle / spinner colors |
| `--toastrack-action-bg` / `--toastrack-action-color` / `--toastrack-action-border` | `#1f2937` / `#ffffff` / `#9ca3af` | Action button colors |

## API overview

`IToastService` (registered by `AddToastRack()`, scoped):

| Member | Description |
|---|---|
| `Success` / `Warning` / `Error` / `Info` `(title, caption?, position?)` | One-line shorthands |
| `Loading(id, title, caption?, showProgress?, position?)` / `Progress(id, percentage)` / `Resolve(id, variant?, title?, caption?, position?)` | Loading-toast shorthands |
| `ShowToast(ToastOptions)` | Show a toast (default variant `Success`) |
| `ShowSuccessToast` / `ShowWarningToast` / `ShowErrorToast` / `ShowInfoToast` | Variant shorthands |
| `ShowLoadingToast(LoadingToastOptions)` | Loading toast at any position, never auto-expires |
| `UpdateLoadingToastProgress(ToastProgressUpdate)` | Update a progress loading toast (5–100 %) |
| `ResolveLoadingToast(ResolveToastOptions)` | Replace a loading toast with a result toast (keeps its position by default) |
| `RemoveToast(string)` / `RemoveToast(ToastItem)` | Remove programmatically |
| `Toasts` / `LoadingToasts` / `ToastsByPosition` | Current state (read-only snapshots) |
| `ToastsUpdated` | Raised on every change |
| `Defaults` | The `ToastDefaults` currently applied to toasts that leave a property unset |
| `SetDefaults(ToastDefaults)` | Replaces those defaults — called by `<ToastRack />` from its parameters, so you rarely call it yourself |

`<ToastRack />` parameters (all optional):

| Parameter | Default | Purpose |
|---|---|---|
| `BoundarySelector` | `null` (viewport) | CSS selector of the element toasts are anchored within |
| `Position` | `BottomLeft` | Position for toasts that leave `ToastOptions.Position` unset |
| `CloseByClick` | `true` | Click-to-dismiss for toasts that leave `ToastOptions.CloseByClick` unset |
| `Expiry` | `5` | Expiry in seconds for toasts that leave `ToastOptions.Expiry` unset |

Behavior notes:

- Toasts with actions never auto-expire; everything else uses the rack's `Expiry` default
  (5 seconds unless set), which `ToastOptions.Expiry` overrides per toast.
- `ToastOptions.Position`, `CloseByClick` and `Expiry` are nullable: leave one `null` to inherit the
  matching `<ToastRack />` parameter, or set it to override the rack for that toast.
  `LoadingToastOptions.Position` is nullable and inherits the same way.
- Each toast captures the defaults in force at the moment it is shown. Changing the rack's
  parameters therefore affects subsequent toasts, not those already on screen.
- The values behind the rack's parameters live on `ToastDefaults`, reachable as
  `IToastService.Defaults` if you need to read the effective default.
- Hovering a toast pauses its expiry timer (except loading toasts, which don't expire).
- The service is thread-safe, so background work can show and update toasts (in Blazor Server too).

## Links

- **Documentation and live demo:** https://lisihasaj.github.io/ToastRack/
- **Source, issues and contributing:** https://github.com/lisihasaj/ToastRack
- **Changelog:** https://github.com/lisihasaj/ToastRack/blob/main/CHANGELOG.md
- **License:** [MIT](https://github.com/lisihasaj/ToastRack/blob/main/LICENSE)
