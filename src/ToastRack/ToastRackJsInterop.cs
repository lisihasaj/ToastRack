using Microsoft.JSInterop;

namespace ToastRack;

/// <summary>
/// Thin wrapper over the package's JS module (<c>wwwroot/toastrack.js</c>). The module is
/// imported lazily on first use, and all calls swallow <see cref="JSDisconnectedException"/>
/// so prerendering and circuit teardown never surface interop errors.
/// </summary>
internal sealed class ToastRackJsInterop(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _module = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/ToastRack/toastrack.js").AsTask());

    public async Task ScrollToBottomAsync(string elementId)
    {
        try
        {
            var module = await _module.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("scrollToBottom", elementId).ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // Circuit gone; nothing to scroll.
        }
    }

    public async Task<bool> ObserveBoundaryAsync<T>(
        string selector, DotNetObjectReference<T> dotNetRef, string methodName)
        where T : class
    {
        try
        {
            var module = await _module.Value.ConfigureAwait(false);
            return await module.InvokeAsync<bool>("observeBoundary", selector, dotNetRef, methodName)
                .ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            return false;
        }
    }

    public async Task UnobserveBoundaryAsync()
    {
        try
        {
            var module = await _module.Value.ConfigureAwait(false);
            await module.InvokeVoidAsync("unobserveBoundary").ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // Circuit gone.
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_module.IsValueCreated) return;

        try
        {
            var module = await _module.Value.ConfigureAwait(false);
            await module.DisposeAsync().ConfigureAwait(false);
        }
        catch (JSDisconnectedException)
        {
            // Circuit gone; the browser cleaned up with it.
        }
    }
}
