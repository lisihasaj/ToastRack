namespace ToastRack;

/// <summary>
/// The fallback values applied to a toast when the corresponding <see cref="ToastOptions"/>
/// property is left <c>null</c>. Supplied by the <c>ToastRack</c> component through
/// <see cref="IToastService.SetDefaults"/>; the property initializers here are the values used
/// when the component leaves the matching parameter unset.
/// </summary>
public class ToastDefaults
{
    /// <summary>
    /// The position used by toasts that do not specify one. Defaults to
    /// <see cref="ToastPosition.BottomLeft"/>.
    /// </summary>
    public ToastPosition Position { get; set; } = ToastPosition.BottomLeft;

    /// <summary>
    /// Whether clicking a toast dismisses it, for toasts that do not specify it.
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool CloseByClick { get; set; } = true;

    /// <summary>
    /// Time in seconds before a toast that does not specify an expiry auto-closes.
    /// Defaults to <c>5</c>.
    /// </summary>
    public int Expiry { get; set; } = 5;
}
