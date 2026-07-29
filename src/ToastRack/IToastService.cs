namespace ToastRack;

/// <summary>
/// Manages the toasts displayed by the <c>ToastRackHost</c> component. Register via
/// <c>services.AddToastRack()</c> and place a single <c>&lt;ToastRackHost /&gt;</c> in your layout.
/// </summary>
public interface IToastService
{
    /// <summary>Raised whenever the set of active toasts changes.</summary>
    event Action? ToastsUpdated;

    /// <summary>The currently active regular (non-loading) toasts, in insertion order.</summary>
    IReadOnlyList<ToastItem> Toasts { get; }

    /// <summary>The currently active loading toasts, in insertion order.</summary>
    IReadOnlyList<ToastItem> LoadingToasts { get; }

    /// <summary>
    /// The active regular toasts grouped by <see cref="ToastPosition"/>. Bottom-anchored groups
    /// are returned newest-first so new toasts visually appear closest to the edge.
    /// </summary>
    IReadOnlyDictionary<ToastPosition, IReadOnlyList<ToastItem>> ToastsByPosition { get; }

    /// <summary>
    /// Shows a toast. If <see cref="ToastOptions.ToastId"/> matches an active toast,
    /// the call is ignored (dedupe). Error toasts and toasts with actions never auto-expire.
    /// </summary>
    void ShowToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Success"/>.</summary>
    void ShowSuccessToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Warning"/>.</summary>
    void ShowWarningToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Error"/>. Error toasts never auto-expire.</summary>
    void ShowErrorToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Info"/>.</summary>
    void ShowInfoToast(ToastOptions options);

    /// <summary>
    /// Shows a loading toast (top-center, never auto-expires). Resolve it with
    /// <see cref="ResolveLoadingToast"/> or remove it with <see cref="RemoveToast(string)"/>.
    /// </summary>
    void ShowLoadingToast(LoadingToastOptions options);

    /// <summary>
    /// Updates the progress circle of a loading toast created with
    /// <see cref="LoadingToastOptions.IsProgress"/> set to <c>true</c>.
    /// </summary>
    void UpdateLoadingToastProgress(ToastProgressUpdate update);

    /// <summary>
    /// Replaces an active loading toast with a regular toast of the variant given by
    /// <see cref="ResolveToastOptions.ReplaceWith"/>.
    /// </summary>
    void ResolveLoadingToast(ResolveToastOptions options);

    /// <summary>Removes the given toast (regular or loading).</summary>
    void RemoveToast(ToastItem toast);

    /// <summary>Removes the toast (regular or loading) with the given id, if it exists.</summary>
    void RemoveToast(string toastId);
}
