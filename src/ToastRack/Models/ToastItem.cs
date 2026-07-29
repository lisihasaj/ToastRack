namespace ToastRack;

/// <summary>
/// A toast currently managed by the <see cref="IToastService"/>. Extends the original
/// <see cref="ToastOptions"/> with runtime state.
/// </summary>
public class ToastItem : ToastOptions
{
    /// <summary>
    /// The absolute point in time at which the toast auto-closes.
    /// Set to <see cref="DateTimeOffset.MaxValue"/> for toasts that never expire.
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// Whether a loading toast shows a determinate progress circle (filled clockwise by
    /// <see cref="Percentage"/>) in place of the indeterminate spinner. Only applies to
    /// loading toasts.
    /// </summary>
    public bool IsProgress { get; set; }

    /// <summary>
    /// The current progress of a loading toast, from <c>0</c> to <c>100</c>, used to fill the
    /// progress circle clockwise. Only applies when <see cref="IsProgress"/> is <c>true</c>.
    /// </summary>
    public int Percentage { get; set; }
}
