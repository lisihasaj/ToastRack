using AngleSharp.Dom;
using Microsoft.AspNetCore.Components.Web;

namespace ToastRack.Tests;

/// <summary>
/// bUnit ships Click() etc. but no helpers for onmouseenter/onmouseleave.
/// </summary>
internal static class ElementExtensions
{
    public static void MouseEnter(this IElement element) =>
        element.TriggerEventAsync("onmouseenter", new MouseEventArgs()).GetAwaiter().GetResult();

    public static void MouseLeave(this IElement element) =>
        element.TriggerEventAsync("onmouseleave", new MouseEventArgs()).GetAwaiter().GetResult();
}
