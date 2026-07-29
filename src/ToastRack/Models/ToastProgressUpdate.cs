namespace ToastRack;

/// <summary>
/// A progress update for a loading toast, applied via
/// <see cref="IToastService.UpdateLoadingToastProgress"/>.
/// </summary>
public class ToastProgressUpdate
{
    /// <summary>
    /// Identifier of the loading toast whose progress is updated. If no loading toast with this
    /// id exists, the update is ignored.
    /// </summary>
    public required string? ToastId { get; set; }

    /// <summary>
    /// The progress value the toast's progress circle fills to, from <c>0</c> to <c>100</c>.
    /// Values outside this range are clamped.
    /// </summary>
    public int Percentage { get; set; }
}
