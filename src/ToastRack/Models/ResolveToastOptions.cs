namespace ToastRack;

/// <summary>
/// Options describing how a loading toast is resolved into a regular toast via
/// <see cref="IToastService.ResolveLoadingToast"/>.
/// </summary>
public class ResolveToastOptions
{
    /// <summary>
    /// Identifier of the loading toast to resolve. Must match the id of an existing loading toast.
    /// </summary>
    public string ToastId { get; set; } = string.Empty;

    /// <summary>
    /// The variant the loading toast is replaced with once resolved (e.g. success, error).
    /// </summary>
    public ToastVariant ReplaceWith { get; set; } = ToastVariant.Success;

    /// <summary>
    /// The position of the resolved toast. Defaults to <see cref="ToastPosition.BottomLeft"/>.
    /// </summary>
    public ToastPosition Position { get; set; } = ToastPosition.BottomLeft;

    /// <summary>
    /// The heading text for the resolved toast.
    /// Leave <c>null</c> to use the default title for the <see cref="ReplaceWith"/> variant.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The secondary descriptive text for the resolved toast.
    /// Leave <c>null</c> to use the default caption for the <see cref="ReplaceWith"/> variant.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Time in seconds before the resolved toast expires and auto-closes.
    /// For example, a value of <c>4</c> means the toast disappears after 4 seconds.
    /// </summary>
    public int Expiry { get; set; } = 5;
}
