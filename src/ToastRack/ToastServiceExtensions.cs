namespace ToastRack;

/// <summary>
/// One-line shorthands for <see cref="IToastService"/>. Use these for the common cases;
/// switch to the options-based methods when you need actions, custom icons, expiry control
/// or fully custom content.
/// </summary>
public static class ToastServiceExtensions
{
    /// <summary>Shows a success toast with the given texts.</summary>
    public static void Success(
        this IToastService service,
        string title,
        string? caption = null,
        ToastPosition? position = null) =>
        service.ShowSuccessToast(new ToastOptions { Title = title, Caption = caption, Position = position });

    /// <summary>Shows a warning toast with the given texts.</summary>
    public static void Warning(
        this IToastService service,
        string title,
        string? caption = null,
        ToastPosition? position = null) =>
        service.ShowWarningToast(new ToastOptions { Title = title, Caption = caption, Position = position });

    /// <summary>Shows an error toast with the given texts.</summary>
    public static void Error(
        this IToastService service,
        string title,
        string? caption = null,
        ToastPosition? position = null) =>
        service.ShowErrorToast(new ToastOptions { Title = title, Caption = caption, Position = position });

    /// <summary>Shows an info toast with the given texts.</summary>
    public static void Info(
        this IToastService service,
        string title,
        string? caption = null,
        ToastPosition? position = null) =>
        service.ShowInfoToast(new ToastOptions { Title = title, Caption = caption, Position = position });

    /// <summary>
    /// Shows a loading toast (never auto-expires). Pass <paramref name="showProgress"/>
    /// as <c>true</c> for a determinate progress circle driven by
    /// <see cref="Progress(IToastService, string, int)"/>.
    /// </summary>
    public static void Loading(
        this IToastService service,
        string toastId,
        string title,
        string? caption = null,
        bool showProgress = false,
        ToastPosition? position = null) =>
        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = toastId,
            Title = title,
            Caption = caption,
            IsProgress = showProgress,
            Position = position,
        });

    /// <summary>Updates the progress circle of the loading toast with the given id.</summary>
    public static void Progress(this IToastService service, string toastId, int percentage) =>
        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = toastId,
            Percentage = percentage,
        });

    /// <summary>
    /// Replaces the loading toast with the given id by a regular result toast. The result toast
    /// keeps the loading toast's position unless <paramref name="position"/> is given.
    /// </summary>
    public static void Resolve(
        this IToastService service,
        string toastId,
        ToastVariant result = ToastVariant.Success,
        string? title = null,
        string? caption = null,
        ToastPosition? position = null) =>
        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = toastId,
            ReplaceWith = result,
            Title = title,
            Caption = caption,
            Position = position,
        });
}
