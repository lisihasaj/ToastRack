using Microsoft.AspNetCore.Components;

namespace ToastRack;

/// <summary>
/// Options describing a toast shown via <see cref="IToastService.ShowToast"/>.
/// </summary>
public class ToastOptions
{
    /// <summary>
    /// Unique identifier for the toast. Used to prevent duplicate toasts and to
    /// target the toast for later removal. Leave <c>null</c> to auto-generate a new id.
    /// </summary>
    public string? ToastId { get; set; }

    /// <summary>
    /// The visual style and semantic meaning of the toast (e.g. success, warning, error).
    /// </summary>
    public ToastVariant Variant { get; set; } = ToastVariant.Success;

    /// <summary>
    /// The screen corner or edge where the toast is displayed. Leave <c>null</c> to use the
    /// default position configured on the <c>ToastRack</c> component.
    /// </summary>
    public ToastPosition? Position { get; set; }

    /// <summary>
    /// Optional custom icon rendered in place of the default icon for the <see cref="Variant"/>.
    /// Leave <c>null</c> to use the built-in icon.
    /// </summary>
    public RenderFragment? Icon { get; set; }

    /// <summary>
    /// The heading text shown at the top of the toast.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The secondary descriptive text shown below the <see cref="Title"/>.
    /// </summary>
    public string? Caption { get; set; }

    /// <summary>
    /// Whether clicking the toast dismisses it. Leave <c>null</c> to use the default
    /// configured on the <c>ToastRack</c> component.
    /// </summary>
    public bool? CloseByClick { get; set; }

    /// <summary>
    /// Time in seconds before the toast expires and auto-closes.
    /// For example, a value of <c>4</c> means the toast disappears after 4 seconds.
    /// Leave <c>null</c> to use the default expiry configured on the <c>ToastRack</c> component.
    /// Toasts with <see cref="Actions"/> never auto-expire.
    /// </summary>
    public int? Expiry { get; set; }

    /// <summary>
    /// Optional action buttons rendered in the toast, allowing the user to respond to it
    /// (e.g. undo or retry). When one or more actions are provided, the toast does not
    /// auto-expire so the user has time to interact. Leave <c>null</c> for a toast without actions.
    /// </summary>
    public List<ToastAction>? Actions { get; set; }

    /// <summary>
    /// Custom content rendered inside the toast when <see cref="Variant"/> is
    /// <see cref="ToastVariant.Custom"/>. When set, the toast only applies its width, height,
    /// border-radius, box-shadow, transition and overflow styling; everything else (layout,
    /// padding, background, icon, etc.) is managed by this fragment. Leave <c>null</c> for
    /// non-custom toasts.
    /// </summary>
    public RenderFragment? Fragment { get; set; }
}
