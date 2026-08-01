using Microsoft.JSInterop;

namespace ToastRack.Tests;

public class ToastRackJsInteropTests
{
    private readonly FakeJsRuntime _jsRuntime = new();

    private ToastRackJsInterop CreateInterop() => new(_jsRuntime);

    [Fact]
    public async Task ScrollToBottomAsync_InvokesModuleFunction()
    {
        await using var interop = CreateInterop();

        await interop.ScrollToBottomAsync("toastrackTopLeft");

        var invocation = Assert.Single(_jsRuntime.Module.Invocations);
        Assert.Equal("scrollToBottom", invocation.Identifier);
        Assert.Equal("toastrackTopLeft", Assert.Single(invocation.Args!));
    }

    [Fact]
    public async Task Module_IsImportedOnlyOnce()
    {
        await using var interop = CreateInterop();

        await interop.ScrollToBottomAsync("a");
        await interop.ScrollToBottomAsync("b");
        await interop.UnobserveBoundaryAsync();

        Assert.Equal(1, _jsRuntime.ImportCount);
    }

    [Fact]
    public async Task ObserveBoundaryAsync_PassesArgumentsAndReturnsModuleResult()
    {
        await using var interop = CreateInterop();
        _jsRuntime.Module.NextResult = true;
        using var dotNetRef = DotNetObjectReference.Create(this);

        var observed = await interop.ObserveBoundaryAsync("main", dotNetRef, "OnBoundaryChanged");

        Assert.True(observed);
        var invocation = Assert.Single(_jsRuntime.Module.Invocations);
        Assert.Equal("observeBoundary", invocation.Identifier);
        Assert.Equal(new object?[] { "main", dotNetRef, "OnBoundaryChanged" }, invocation.Args);
    }

    [Fact]
    public async Task UnobserveBoundaryAsync_InvokesModuleFunction()
    {
        await using var interop = CreateInterop();

        await interop.UnobserveBoundaryAsync();

        var invocation = Assert.Single(_jsRuntime.Module.Invocations);
        Assert.Equal("unobserveBoundary", invocation.Identifier);
    }

    [Fact]
    public async Task DisposeAsync_DisposesModule()
    {
        var interop = CreateInterop();
        await interop.ScrollToBottomAsync("a");

        await interop.DisposeAsync();

        Assert.True(_jsRuntime.Module.Disposed);
    }

    [Fact]
    public async Task DisposeAsync_WithoutAnyCall_DoesNotImportModule()
    {
        var interop = CreateInterop();

        await interop.DisposeAsync();

        Assert.Equal(0, _jsRuntime.ImportCount);
        Assert.False(_jsRuntime.Module.Disposed);
    }

    [Fact]
    public async Task ScrollToBottomAsync_SwallowsJsDisconnectedException()
    {
        await using var interop = CreateInterop();
        _jsRuntime.Module.ThrowOnInvoke = new JSDisconnectedException("circuit gone");

        await interop.ScrollToBottomAsync("toastrackTopLeft");
    }

    [Fact]
    public async Task ObserveBoundaryAsync_ReturnsFalseWhenDisconnected()
    {
        await using var interop = CreateInterop();
        _jsRuntime.Module.ThrowOnInvoke = new JSDisconnectedException("circuit gone");
        using var dotNetRef = DotNetObjectReference.Create(this);

        var observed = await interop.ObserveBoundaryAsync("main", dotNetRef, "OnBoundaryChanged");

        Assert.False(observed);
    }

    [Fact]
    public async Task UnobserveBoundaryAsync_SwallowsJsDisconnectedException()
    {
        await using var interop = CreateInterop();
        _jsRuntime.Module.ThrowOnInvoke = new JSDisconnectedException("circuit gone");

        await interop.UnobserveBoundaryAsync();
    }

    [Fact]
    public async Task DisposeAsync_SwallowsJsDisconnectedExceptionFromModuleDispose()
    {
        var interop = CreateInterop();
        await interop.ScrollToBottomAsync("a");
        _jsRuntime.Module.ThrowOnDispose = new JSDisconnectedException("circuit gone");

        await interop.DisposeAsync();
    }

    [Fact]
    public async Task AllCalls_SwallowJsDisconnectedExceptionFromImport()
    {
        _jsRuntime.ThrowOnImport = new JSDisconnectedException("circuit gone");
        var interop = CreateInterop();
        using var dotNetRef = DotNetObjectReference.Create(this);

        await interop.ScrollToBottomAsync("a");
        Assert.False(await interop.ObserveBoundaryAsync("main", dotNetRef, "OnBoundaryChanged"));
        await interop.UnobserveBoundaryAsync();
        // The lazy import was attempted (and failed), so disposal awaits it and must also swallow.
        await interop.DisposeAsync();

        Assert.Empty(_jsRuntime.Module.Invocations);
    }

    /// <summary>
    /// Minimal <see cref="IJSRuntime"/> stub: serves the module for <c>import</c> and
    /// fails the import on demand, so the interop's exception handling can be exercised
    /// without a browser.
    /// </summary>
    private sealed class FakeJsRuntime : IJSRuntime
    {
        public FakeJsModule Module { get; } = new();

        public int ImportCount { get; private set; }

        public JSDisconnectedException? ThrowOnImport { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Assert.Equal("import", identifier);
            Assert.Equal("./_content/ToastRack/toastrack.js", Assert.Single(args!));

            ImportCount++;
            if (ThrowOnImport is not null) throw ThrowOnImport;
            return ValueTask.FromResult((TValue)(object)Module);
        }
    }

    /// <summary>Records module invocations and throws on demand, like a disconnected circuit.</summary>
    private sealed class FakeJsModule : IJSObjectReference
    {
        public List<(string Identifier, object?[]? Args)> Invocations { get; } = [];

        public object? NextResult { get; set; }

        public bool Disposed { get; private set; }

        public JSDisconnectedException? ThrowOnInvoke { get; set; }

        public JSDisconnectedException? ThrowOnDispose { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
            InvokeAsync<TValue>(identifier, CancellationToken.None, args);

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (ThrowOnInvoke is not null) throw ThrowOnInvoke;
            Invocations.Add((identifier, args));
            return ValueTask.FromResult(NextResult is TValue value ? value : default!);
        }

        public ValueTask DisposeAsync()
        {
            if (ThrowOnDispose is not null) throw ThrowOnDispose;
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
