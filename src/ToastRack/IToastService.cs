namespace ToastRack;

/// <summary>
/// Manages the toasts displayed by the <c>ToastRack</c> component. Register via
/// <c>services.AddToastRack()</c> and place a single <c>&lt;ToastRack /&gt;</c> in your layout.
/// </summary>
public interface IToastService
{
    /// <summary>Raised whenever the set of active toasts changes.</summary>
    event Action? ToastsUpdated;

    /// <summary>
    /// The fallback values applied to toasts that leave the corresponding
    /// <see cref="ToastOptions"/> property <c>null</c>. Set by the <c>ToastRack</c> component
    /// from its parameters via <see cref="SetDefaults"/>.
    /// </summary>
    ToastDefaults Defaults { get; }

    /// <summary>
    /// Replaces the defaults applied to subsequently shown toasts. Called by the
    /// <c>ToastRack</c> component from its parameters; there is normally no need to call it
    /// directly. Toasts already on screen keep the defaults in force when they were shown.
    /// </summary>
    void SetDefaults(ToastDefaults defaults);

    /// <summary>The currently active toasts (loading toasts included), in insertion order.</summary>
    IReadOnlyList<ToastItem> Toasts { get; }

    /// <summary>
    /// The currently active loading toasts, in insertion order. A filtered view of
    /// <see cref="Toasts"/> restricted to <see cref="ToastVariant.Loading"/>.
    /// </summary>
    IReadOnlyList<ToastItem> LoadingToasts { get; }

    /// <summary>
    /// The active toasts grouped by <see cref="ToastPosition"/>. Bottom-anchored groups
    /// are returned newest-first so new toasts visually appear closest to the edge.
    /// </summary>
    IReadOnlyDictionary<ToastPosition, IReadOnlyList<ToastItem>> ToastsByPosition { get; }

    /// <summary>
    /// Shows a toast. If <see cref="ToastOptions.ToastId"/> matches an active toast,
    /// the call is ignored (dedupe). Toasts with actions never auto-expire.
    /// </summary>
    void ShowToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Success"/>.</summary>
    void ShowSuccessToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Warning"/>.</summary>
    void ShowWarningToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Error"/>.</summary>
    void ShowErrorToast(ToastOptions options);

    /// <summary>Shows a toast with <see cref="ToastVariant.Info"/>.</summary>
    void ShowInfoToast(ToastOptions options);

    /// <summary>
    /// Shows a loading toast. It stacks at its <see cref="LoadingToastOptions.Position"/> like
    /// any other toast but never auto-expires. Resolve it with <see cref="ResolveLoadingToast"/>
    /// or remove it with <see cref="RemoveToast(string)"/>.
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
