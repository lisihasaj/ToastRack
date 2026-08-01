namespace ToastRack;

/// <summary>
/// Default implementation of <see cref="IToastService"/>. Thread-safe: list mutations are
/// guarded by a lock so the service can be used from Blazor Server circuits and background
/// threads; <see cref="ToastsUpdated"/> is raised outside the lock.
/// </summary>
public class ToastService : IToastService
{
    private const int DefaultExpirySeconds = 5;
    private const int InitialProgressPercentage = 5;

    private readonly object _gate = new();
    private readonly List<ToastItem> _toasts = [];

    /// <inheritdoc />
    public event Action? ToastsUpdated;

    /// <inheritdoc />
    public IReadOnlyList<ToastItem> Toasts
    {
        get
        {
            lock (_gate)
            {
                return _toasts.ToArray();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<ToastItem> LoadingToasts
    {
        get
        {
            lock (_gate)
            {
                return _toasts.Where(t => t.Variant is ToastVariant.Loading).ToArray();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<ToastPosition, IReadOnlyList<ToastItem>> ToastsByPosition
    {
        get
        {
            lock (_gate)
            {
                return _toasts
                    .GroupBy(t => t.Position)
                    .OrderBy(g => g.Key)
                    .ToDictionary(
                        g => g.Key,
                        g => (IReadOnlyList<ToastItem>)(IsBottomPosition(g.Key)
                            ? g.Reverse().ToList()
                            : g.ToList()));
            }
        }
    }

    /// <inheritdoc />
    public void ShowToast(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var noExpiry = options.Actions is not null && options.Actions.Count > 0;

        lock (_gate)
        {
            if (options.ToastId != null && _toasts.Any(t => t.ToastId == options.ToastId)) return;

            var toastItem = new ToastItem
            {
                ToastId = options.ToastId ?? Guid.NewGuid().ToString(),
                Variant = options.Variant,
                Position = options.Position,
                Icon = options.Icon,
                Title = options.Title,
                Caption = options.Caption,
                CloseByClick = options.CloseByClick,
                ExpiresAt = noExpiry
                    ? DateTimeOffset.MaxValue
                    : DateTimeOffset.UtcNow.AddSeconds(options.Expiry ?? DefaultExpirySeconds),
                Expiry = noExpiry ? null : options.Expiry,
                Actions = options.Actions,
                Fragment = options.Fragment,
            };

            _toasts.Add(toastItem);
        }

        NotifyToastsUpdated();
    }

    /// <inheritdoc />
    public void ShowSuccessToast(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Variant = ToastVariant.Success;
        ShowToast(options);
    }

    /// <inheritdoc />
    public void ShowWarningToast(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Variant = ToastVariant.Warning;
        ShowToast(options);
    }

    /// <inheritdoc />
    public void ShowErrorToast(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Variant = ToastVariant.Error;
        ShowToast(options);
    }

    /// <inheritdoc />
    public void ShowInfoToast(ToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Variant = ToastVariant.Info;
        ShowToast(options);
    }

    /// <inheritdoc />
    public void ShowLoadingToast(LoadingToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_gate)
        {
            if (options.ToastId != null && _toasts.Any(t => t.ToastId == options.ToastId)) return;

            var toastItem = new ToastItem
            {
                ToastId = options.ToastId ?? Guid.NewGuid().ToString(),
                Variant = ToastVariant.Loading,
                Position = options.Position,
                Title = options.Title,
                Caption = options.Caption,
                CloseByClick = false,
                ExpiresAt = DateTimeOffset.MaxValue,
                Expiry = null,
                IsProgress = options.IsProgress,
                Percentage = options.IsProgress ? InitialProgressPercentage : 0,
            };

            _toasts.Add(toastItem);
        }

        NotifyToastsUpdated();
    }

    /// <inheritdoc />
    public void UpdateLoadingToastProgress(ToastProgressUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        lock (_gate)
        {
            var toast = _toasts.Find(t => t.Variant is ToastVariant.Loading && t.ToastId == update.ToastId);
            if (toast is null) return;

            toast.Percentage = Math.Max(InitialProgressPercentage, Math.Clamp(update.Percentage, 0, 100));
        }

        NotifyToastsUpdated();
    }

    /// <inheritdoc />
    public void ResolveLoadingToast(ResolveToastOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        lock (_gate)
        {
            var toast = _toasts.Find(t => t.Variant is ToastVariant.Loading && t.ToastId == options.ToastId);
            if (toast is null) return;

            var (defaultTitle, defaultCaption) = options.ReplaceWith switch
            {
                ToastVariant.Success => ("Success", "Task is completed successfully."),
                ToastVariant.Error => ("Error", "An error occurred."),
                ToastVariant.Warning => ("Warning", "Task completed with warning."),
                _ => ("Information", "Task is completed."),
            };

            toast.Variant = options.ReplaceWith is ToastVariant.Success
                or ToastVariant.Error
                or ToastVariant.Warning
                ? options.ReplaceWith
                : ToastVariant.Info;
            toast.Position = options.Position ?? toast.Position;
            toast.Title = options.Title ?? defaultTitle;
            toast.Caption = options.Caption ?? defaultCaption;
            toast.CloseByClick = true;
            toast.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(options.Expiry);
            toast.Expiry = options.Expiry;
            toast.IsProgress = false;
            toast.Percentage = 0;
        }

        NotifyToastsUpdated();
    }

    /// <inheritdoc />
    public void RemoveToast(ToastItem toast)
    {
        ArgumentNullException.ThrowIfNull(toast);

        lock (_gate)
        {
            _toasts.Remove(toast);
        }

        NotifyToastsUpdated();
    }

    /// <inheritdoc />
    public void RemoveToast(string toastId)
    {
        lock (_gate)
        {
            var toast = _toasts.Find(t => t.ToastId == toastId);
            if (toast is null) return;

            _toasts.Remove(toast);
        }

        NotifyToastsUpdated();
    }

    private static bool IsBottomPosition(ToastPosition position) =>
        position is ToastPosition.BottomLeft
            or ToastPosition.BottomRight
            or ToastPosition.BottomCenter;

    private void NotifyToastsUpdated() => ToastsUpdated?.Invoke();
}
