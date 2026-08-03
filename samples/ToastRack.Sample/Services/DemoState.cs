namespace ToastRack.Sample.Services;

/// <summary>
/// Demo-wide UI state shared between the layout and the demo page: the CSS selector the
/// <c>ToastRack</c> component is currently bound to and the active toast theme class.
/// </summary>
public sealed class DemoState
{
    /// <summary>Raised whenever any demo state changes.</summary>
    public event Action? Changed;

    /// <summary>
    /// CSS selector of the element toasts are currently anchored to,
    /// or <c>null</c> to anchor them to the viewport.
    /// </summary>
    public string? BoundarySelector { get; private set; }

    /// <summary>CSS class applied around the app that themes the toasts via <c>--toastrack-*</c> variables.</summary>
    public string ThemeClass { get; private set; } = "";

    /// <summary>
    /// Default position passed to the <c>ToastRack</c> component, applied to toasts that do not
    /// set <see cref="ToastOptions.Position"/> themselves.
    /// </summary>
    public ToastPosition? DefaultPosition { get; private set; }

    /// <summary>
    /// Default click-to-dismiss behaviour passed to the <c>ToastRack</c> component, applied to
    /// toasts that do not set <see cref="ToastOptions.CloseByClick"/> themselves.
    /// </summary>
    public bool? DefaultCloseByClick { get; private set; }

    /// <summary>
    /// Default expiry in seconds passed to the <c>ToastRack</c> component, applied to toasts that
    /// do not set <see cref="ToastOptions.Expiry"/> themselves.
    /// </summary>
    public int? DefaultExpiry { get; private set; }

    /// <summary>Sets the boundary selector and notifies subscribers.</summary>
    public void SetBoundary(string? selector)
    {
        BoundarySelector = selector;
        Changed?.Invoke();
    }

    /// <summary>Sets the toast theme class and notifies subscribers.</summary>
    public void SetTheme(string themeClass)
    {
        ThemeClass = themeClass;
        Changed?.Invoke();
    }

    /// <summary>Sets the rack-level toast defaults and notifies subscribers.</summary>
    public void SetRackDefaults(ToastPosition? position, bool? closeByClick, int? expiry)
    {
        DefaultPosition = position;
        DefaultCloseByClick = closeByClick;
        DefaultExpiry = expiry;
        Changed?.Invoke();
    }
}
