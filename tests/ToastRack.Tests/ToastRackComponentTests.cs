using System.Reflection;
using Bunit.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using ToastRackComponent = ToastRack.Components.ToastRack;

namespace ToastRack.Tests;

public class ToastRackComponentTests : BunitContext
{
    private readonly ToastService _toastService = new();
    private readonly BunitJSModuleInterop _module;

    public ToastRackComponentTests()
    {
        Services.AddSingleton<IToastService>(_toastService);
        _module = JSInterop.SetupModule("./_content/ToastRack/toastrack.js");
        _module.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersNothing_WhenNoToasts()
    {
        var component = Render<ToastRackComponent>();

        Assert.DoesNotContain("toastrack--", component.Markup);
    }

    [Fact]
    public void RendersToastInPositionGroupWithCorrectClass()
    {
        _toastService.ShowSuccessToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Saved",
            Position = ToastPosition.BottomCenter,
        });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack--bottom-center", component.Markup);
        Assert.Contains("id=\"toastrackBottomCenter\"", component.Markup);
        Assert.Contains("Saved", component.Markup);
        // Bottom positions slide up.
        Assert.Contains("toastrack-animate-slide-up", component.Markup);
    }

    [Fact]
    public void TopPositionUsesSlideDownAnimation()
    {
        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t-top",
            Title = "Info",
            Position = ToastPosition.TopRight,
        });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack--top-right", component.Markup);
        Assert.Contains("toastrack-animate-slide-down", component.Markup);
    }

    [Fact]
    public void TopCenterPosition_RendersOwnGroupWithSlideDown()
    {
        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t-center",
            Title = "Centered",
            Position = ToastPosition.TopCenter,
        });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack--top-center", component.Markup);
        Assert.Contains("id=\"toastrackTopCenter\"", component.Markup);
        Assert.Contains("toastrack-animate-slide-down", component.Markup);
    }

    [Fact]
    public void GroupsToastsByPosition()
    {
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "a", Title = "TopLeftToast", Position = ToastPosition.TopLeft });
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "b", Title = "BottomRightToast", Position = ToastPosition.BottomRight });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack--top-left", component.Markup);
        Assert.Contains("toastrack--bottom-right", component.Markup);
        Assert.Equal(2, component.FindAll(".toastrack").Count);
    }

    [Fact]
    public void ReRendersWhenToastAdded()
    {
        var component = Render<ToastRackComponent>();
        Assert.DoesNotContain("toastrack--", component.Markup);

        // Adding a toast raises ToastsUpdated, which the component subscribes to.
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "late", Title = "LateToast" });

        component.WaitForAssertion(() => Assert.Contains("LateToast", component.Markup));
    }

    [Fact]
    public void LoadingToast_RendersInItsPositionGroup()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "l1",
            Title = "Loading one",
            Position = ToastPosition.TopCenter,
        });

        var component = Render<ToastRackComponent>();

        Assert.Contains("id=\"toastrackTopCenter\"", component.Markup);
        Assert.Contains("toastrack--top-center", component.Markup);
        Assert.Contains("Loading one", component.Markup);
        Assert.Contains("toastrack-toast--loading", component.Markup);
    }

    [Fact]
    public void LoadingToast_SharesGroupWithOtherVariants()
    {
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "i1", Title = "InfoFirst", Position = ToastPosition.TopRight });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "LoadingSecond", Position = ToastPosition.TopRight });

        var component = Render<ToastRackComponent>();

        // One shared container holds both toasts in insertion order.
        var container = component.Find("#toastrackTopRight");
        var toasts = container.QuerySelectorAll(".toastrack-toast");
        Assert.Equal(2, toasts.Length);
        Assert.Contains("InfoFirst", toasts[0].TextContent);
        Assert.Contains("LoadingSecond", toasts[1].TextContent);
    }

    [Fact]
    public void MultipleLoadingToasts_RenderIndividually()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackComponent>();

        // No collapsing: every loading toast is visible on its own.
        Assert.Contains("First", component.Markup);
        Assert.Contains("Second", component.Markup);
        Assert.Equal(2, component.FindAll(".toastrack-toast--loading").Count);
    }

    [Fact]
    public void SingleProgressToast_RendersProgressCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Exporting", IsProgress = true });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack-toast__progress-circle", component.Markup);
        Assert.Contains("--toastrack-progress: 5%", component.Markup);
    }

    [Fact]
    public void ProgressUpdate_ReRendersCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Exporting", IsProgress = true });

        var component = Render<ToastRackComponent>();

        _toastService.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "l1",
            Percentage = 65,
        });

        component.WaitForAssertion(() => Assert.Contains("--toastrack-progress: 65%", component.Markup));
    }

    [Fact]
    public void MultipleProgressToasts_EachRenderTheirOwnCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First", IsProgress = true });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second", IsProgress = true });
        _toastService.UpdateLoadingToastProgress(new ToastProgressUpdate { ToastId = "l2", Percentage = 80 });

        var component = Render<ToastRackComponent>();

        Assert.Equal(2, component.FindAll(".toastrack-toast__progress-circle").Count);
        Assert.Contains("--toastrack-progress: 5%", component.Markup);
        Assert.Contains("--toastrack-progress: 80%", component.Markup);
    }

    [Fact]
    public void ScrollsTopGroupsToBottomAfterRender()
    {
        var component = Render<ToastRackComponent>();

        // Scrolling is triggered by the ToastsUpdated handler, so add the toast after the first render.
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "top", Title = "TopToast", Position = ToastPosition.TopRight });

        component.WaitForAssertion(() =>
            Assert.Contains(
                _module.Invocations,
                i => i.Identifier == "scrollToBottom" && Equals(i.Arguments[0], "toastrackTopRight")));
    }

    [Fact]
    public void ScrollsTopCenterGroupToBottomAfterRender()
    {
        var component = Render<ToastRackComponent>();

        _toastService.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "l1",
            Title = "Loading",
            Position = ToastPosition.TopCenter,
        });

        component.WaitForAssertion(() =>
            Assert.Contains(
                _module.Invocations,
                i => i.Identifier == "scrollToBottom" && Equals(i.Arguments[0], "toastrackTopCenter")));
    }

    [Fact]
    public void WithoutBoundary_DoesNotObserve()
    {
        Render<ToastRackComponent>();

        Assert.DoesNotContain(_module.Invocations, i => i.Identifier == "observeBoundary");
    }

    [Fact]
    public void WithBoundarySelector_ObservesBoundary()
    {
        _module.Setup<bool>("observeBoundary", _ => true).SetResult(true);

        var component = Render<ToastRackComponent>(parameters =>
            parameters.Add(p => p.BoundarySelector, "#main-content"));

        component.WaitForAssertion(() => _module.VerifyInvoke("observeBoundary"));
    }

    [Fact]
    public void BoundaryChange_AppliesInlinePositionStyles()
    {
        _toastService.ShowSuccessToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Saved",
            Position = ToastPosition.BottomRight,
        });

        var component = Render<ToastRackComponent>();

        component.Instance.OnBoundaryChanged(new BoundaryRect
        {
            Left = 100,
            Top = 50,
            Width = 800,
            Height = 600,
        });

        component.WaitForAssertion(() =>
        {
            // BottomRight anchors at the boundary's bottom-right corner (left+width, top+height).
            Assert.Contains("left: 900px; top: 650px; max-height: 600px;", component.Markup);
            Assert.Contains("toastrack--bound", component.Markup);
        });
    }

    [Fact]
    public void BoundaryChange_AnchorsTopCenterAtTopMiddle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "l1",
            Title = "Loading",
            Position = ToastPosition.TopCenter,
        });

        var component = Render<ToastRackComponent>();

        component.Instance.OnBoundaryChanged(new BoundaryRect
        {
            Left = 100,
            Top = 50,
            Width = 800,
            Height = 600,
        });

        component.WaitForAssertion(() =>
            // TopCenter anchors at the boundary's top edge, horizontally centered (left+width/2, top).
            Assert.Contains("left: 500px; top: 50px; max-height: 600px;", component.Markup));
    }

    [Theory]
    [InlineData(ToastPosition.TopLeft, "left: 100px; top: 50px;")]
    [InlineData(ToastPosition.TopRight, "left: 900px; top: 50px;")]
    [InlineData(ToastPosition.BottomLeft, "left: 100px; top: 650px;")]
    [InlineData(ToastPosition.BottomCenter, "left: 500px; top: 650px;")]
    public void BoundaryChange_AnchorsEachPositionToItsCorner(ToastPosition position, string expectedStyle)
    {
        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Anchored",
            Position = position,
        });

        var component = Render<ToastRackComponent>();

        component.Instance.OnBoundaryChanged(new BoundaryRect
        {
            Left = 100,
            Top = 50,
            Width = 800,
            Height = 600,
        });

        component.WaitForAssertion(() =>
            Assert.Contains($"{expectedStyle} max-height: 600px;", component.Markup));
    }

    [Fact]
    public void UnknownPosition_FallsBackToBottomLeftClass()
    {
        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "odd",
            Title = "Odd",
            Position = (ToastPosition)999,
        });

        var component = Render<ToastRackComponent>();

        Assert.Contains("toastrack--bottom-left", component.Markup);
    }

    [Fact]
    public async Task DisposeAsync_UnobservesBoundaryWhenObserved()
    {
        _module.Setup<bool>("observeBoundary", _ => true).SetResult(true);

        var component = Render<ToastRackComponent>(parameters =>
            parameters.Add(p => p.BoundarySelector, "#main-content"));
        component.WaitForAssertion(() => _module.VerifyInvoke("observeBoundary"));

        await ((IAsyncDisposable)component.Instance).DisposeAsync();

        _module.VerifyInvoke("unobserveBoundary");
    }

    [Fact]
    public async Task DisposeAsync_IsSafeOnUninitializedComponent()
    {
        // Blazor disposes components even when initialization was cut short (e.g. a failed
        // SetParametersAsync), so disposal must cope with the JS interop and DotNetObjectReference
        // never having been created. The injected service is assigned via reflection because
        // the component is deliberately never rendered.
        var component = new ToastRackComponent();
        typeof(ToastRackComponent)
            .GetProperty("ToastService", BindingFlags.Instance | BindingFlags.NonPublic)!
            .SetValue(component, _toastService);

        await component.DisposeAsync();
    }

    [Fact]
    public async Task DisposesWithoutError()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackComponent>();

        // Disposal unsubscribes from ToastsUpdated; further updates must not re-render.
        await ((IAsyncDisposable)component.Instance).DisposeAsync();

        _toastService.ShowInfoToast(new ToastOptions { ToastId = "late", Title = "AfterDispose" });
        Assert.DoesNotContain("AfterDispose", component.Markup);
    }
}
