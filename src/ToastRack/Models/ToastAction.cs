using Microsoft.AspNetCore.Components;

namespace ToastRack;

/// <summary>
/// An action button rendered inside a toast, allowing the user to respond to it
/// (e.g. undo or retry). When a toast has one or more actions it does not auto-expire,
/// so the user has time to interact. Invoking an action closes the toast.
/// </summary>
public class ToastAction
{
    /// <summary>Optional id rendered on the underlying button element.</summary>
    public string? Id { get; set; }

    /// <summary>The button label.</summary>
    public string? Label { get; set; }

    /// <summary>Optional content (e.g. an icon) rendered before the label.</summary>
    public RenderFragment? Icon { get; set; }

    /// <summary>Synchronous click callback. Invoked before <see cref="OnClickAsync"/>.</summary>
    public Action? OnClick { get; set; }

    /// <summary>Asynchronous click callback. Invoked after <see cref="OnClick"/>.</summary>
    public Func<Task>? OnClickAsync { get; set; }

    /// <summary>Whether the button is disabled.</summary>
    public bool IsDisabled { get; set; }

    /// <summary>The built-in visual style of the button.</summary>
    public ToastActionStyle Style { get; set; } = ToastActionStyle.Primary;

    /// <summary>Additional CSS class(es) appended to the button.</summary>
    public string? CssClass { get; set; }

    /// <summary>Tooltip shown on hover (rendered as the <c>title</c> attribute).</summary>
    public string? Tooltip { get; set; }
}

/// <summary>
/// Built-in visual styles for <see cref="ToastAction"/> buttons.
/// </summary>
public enum ToastActionStyle
{
    /// <summary>A filled, emphasized button.</summary>
    Primary,

    /// <summary>An outlined button.</summary>
    Outline,

    /// <summary>A borderless text button.</summary>
    Text,
}
