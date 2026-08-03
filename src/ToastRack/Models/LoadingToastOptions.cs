namespace ToastRack;

/// <summary>
/// Options describing a loading toast shown via <see cref="IToastService.ShowLoadingToast"/>.
/// Loading toasts stack at their <see cref="Position"/> alongside every other toast variant,
/// never auto-expire, and are resolved with <see cref="IToastService.ResolveLoadingToast"/>.
/// </summary>
public class LoadingToastOptions
{
    /// <summary>
    /// Unique identifier for the loading toast. Used to prevent duplicate toasts and to
    /// target the toast when resolving or removing it. Leave <c>null</c> to auto-generate a new id.
    /// </summary>
    public string? ToastId { get; set; }

    /// <summary>
    /// The screen corner or edge where the loading toast is displayed. Leave <c>null</c> to use
    /// the default position configured on the <c>ToastRack</c> component.
    /// </summary>
    public ToastPosition? Position { get; set; }

    /// <summary>
    /// The heading text shown at the top of the loading toast.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The secondary descriptive text shown below the <see cref="Title"/>.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// When <c>true</c>, the loading toast renders a determinate progress circle that fills
    /// clockwise according to the percentage set via
    /// <see cref="IToastService.UpdateLoadingToastProgress"/>, instead of the indeterminate
    /// spinner. Defaults to <c>false</c>.
    /// </summary>
    public bool IsProgress { get; set; }
}
