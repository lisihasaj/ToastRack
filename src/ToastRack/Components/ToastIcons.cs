using Microsoft.AspNetCore.Components;

namespace ToastRack.Components;

/// <summary>
/// Built-in inline SVG icons, so the package has no icon-font dependency.
/// All icons inherit <c>currentColor</c> and are decorative (<c>aria-hidden</c>).
/// </summary>
internal static class ToastIcons
{
    private const string Svg =
        "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 24 24\" fill=\"currentColor\" " +
        "width=\"1.25em\" height=\"1.25em\" aria-hidden=\"true\" focusable=\"false\">";

    public static readonly MarkupString Success = new(
        Svg + "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm-1.2 14.5-4.3-4.3 1.4-1.4 2.9 2.9 5.9-5.9 1.4 1.4-7.3 7.3Z\"/></svg>");

    public static readonly MarkupString Warning = new(
        Svg + "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm-1 5h2v6h-2V7Zm0 8h2v2h-2v-2Z\"/></svg>");

    public static readonly MarkupString Error = new(
        Svg + "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm3.6 12.2-1.4 1.4L12 13.4l-2.2 2.2-1.4-1.4L10.6 12 8.4 9.8l1.4-1.4 2.2 2.2 2.2-2.2 1.4 1.4L13.4 12l2.2 2.2Z\"/></svg>");

    public static readonly MarkupString Info = new(
        Svg + "<path d=\"M12 2a10 10 0 1 0 0 20 10 10 0 0 0 0-20Zm-1 5h2v2h-2V7Zm0 4h2v6h-2v-6Z\"/></svg>");

    public static readonly MarkupString Close = new(
        Svg + "<path d=\"m6.4 18.3-1.4-1.4 5.3-5.3-5.3-5.3 1.4-1.4 5.3 5.3 5.3-5.3 1.4 1.4-5.3 5.3 5.3 5.3-1.4 1.4-5.3-5.3-5.3 5.3Z\"/></svg>");

    public static MarkupString For(ToastVariant variant) => variant switch
    {
        ToastVariant.Success => Success,
        ToastVariant.Warning => Warning,
        ToastVariant.Error => Error,
        _ => Info,
    };
}
