namespace ToastRack;

/// <summary>
/// The visual style and semantic meaning of a toast.
/// </summary>
public enum ToastVariant
{
    /// <summary>A positive confirmation toast.</summary>
    Success,

    /// <summary>A cautionary toast.</summary>
    Warning,

    /// <summary>An error toast.</summary>
    Error,

    /// <summary>A neutral informational toast.</summary>
    Info,

    /// <summary>A loading toast with an indeterminate spinner or a determinate progress circle.</summary>
    Loading,

    /// <summary>A toast whose content is fully rendered from <see cref="ToastOptions.Fragment"/>.</summary>
    Custom,
}
