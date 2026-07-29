namespace ToastRack;

/// <summary>
/// The bounding rectangle (viewport coordinates, CSS pixels) of the element toasts are
/// positioned within when <c>ToastRackHost.BoundarySelector</c> is set.
/// </summary>
public sealed class BoundaryRect
{
    /// <summary>Distance from the left edge of the viewport.</summary>
    public double Left { get; set; }

    /// <summary>Distance from the top edge of the viewport.</summary>
    public double Top { get; set; }

    /// <summary>Width of the boundary element.</summary>
    public double Width { get; set; }

    /// <summary>Height of the boundary element.</summary>
    public double Height { get; set; }
}
