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
}
