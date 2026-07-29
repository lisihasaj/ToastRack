using Bunit.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using ToastRack.Components;

namespace ToastRack.Tests;

public class ToastRackHostTests : BunitContext
{
    private readonly ToastService _toastService = new();
    private readonly BunitJSModuleInterop _module;

    public ToastRackHostTests()
    {
        Services.AddSingleton<IToastService>(_toastService);
        _module = JSInterop.SetupModule("./_content/ToastRack/toastrack.js");
        _module.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Host_RendersNothing_WhenNoToasts()
    {
        var component = Render<ToastRackHost>();

        Assert.DoesNotContain("toastrack--", component.Markup);
        Assert.DoesNotContain("toastrack-loading", component.Markup);
    }

    [Fact]
    public void Host_RendersToastInPositionGroupWithCorrectClass()
    {
        _toastService.ShowSuccessToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Saved",
            Position = ToastPosition.BottomCenter,
        });

        var component = Render<ToastRackHost>();

        Assert.Contains("toastrack--bottom-center", component.Markup);
        Assert.Contains("id=\"toastrackBottomCenter\"", component.Markup);
        Assert.Contains("Saved", component.Markup);
        // Bottom positions slide up.
        Assert.Contains("toastrack-animate-slide-up", component.Markup);
    }

    [Fact]
    public void Host_TopPositionUsesSlideDownAnimation()
    {
        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t-top",
            Title = "Info",
            Position = ToastPosition.TopRight,
        });

        var component = Render<ToastRackHost>();

        Assert.Contains("toastrack--top-right", component.Markup);
        Assert.Contains("toastrack-animate-slide-down", component.Markup);
    }

    [Fact]
    public void Host_GroupsToastsByPosition()
    {
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "a", Title = "TopLeftToast", Position = ToastPosition.TopLeft });
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "b", Title = "BottomRightToast", Position = ToastPosition.BottomRight });

        var component = Render<ToastRackHost>();

        Assert.Contains("toastrack--top-left", component.Markup);
        Assert.Contains("toastrack--bottom-right", component.Markup);
        Assert.Equal(2, component.FindAll(".toastrack").Count);
    }

    [Fact]
    public void Host_ReRendersWhenToastAdded()
    {
        var component = Render<ToastRackHost>();
        Assert.DoesNotContain("toastrack--", component.Markup);

        // Adding a toast raises ToastsUpdated, which the component subscribes to.
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "late", Title = "LateToast" });

        component.WaitForAssertion(() => Assert.Contains("LateToast", component.Markup));
    }

    [Fact]
    public void Host_SingleLoadingToast_RendersLoadingContainer()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Loading one" });

        var component = Render<ToastRackHost>();

        Assert.Contains("id=\"toastrack-loading\"", component.Markup);
        Assert.Contains("Loading one", component.Markup);
        // A single loading toast is never collapsed.
        Assert.DoesNotContain("toastrack-toast__count", component.Markup);
    }

    [Fact]
    public void Host_SingleProgressToast_RendersProgressCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Exporting", IsProgress = true });

        var component = Render<ToastRackHost>();

        Assert.Contains("id=\"toastrack-loading\"", component.Markup);
        Assert.Contains("toastrack-toast__progress-circle", component.Markup);
        Assert.Contains("--toastrack-progress: 5%", component.Markup);
    }

    [Fact]
    public void Host_ProgressUpdate_ReRendersCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Exporting", IsProgress = true });

        var component = Render<ToastRackHost>();

        _toastService.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "l1",
            Percentage = 65,
        });

        component.WaitForAssertion(() => Assert.Contains("--toastrack-progress: 65%", component.Markup));
    }

    [Fact]
    public void Host_MultipleProgressToasts_CollapsedPlaceholderShowsSpinner()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First", IsProgress = true });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second", IsProgress = true });

        var component = Render<ToastRackHost>();

        // The collapsed placeholder aggregates several toasts, so it has no single
        // percentage to show and falls back to the indeterminate spinner.
        Assert.Contains("toastrack-spinner", component.Markup);
        Assert.DoesNotContain("toastrack-toast__progress-circle", component.Markup);
    }

    [Fact]
    public void Host_MultipleProgressToasts_ExpandOnHoverShowsEachCircle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First", IsProgress = true });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second", IsProgress = true });
        _toastService.UpdateLoadingToastProgress(new ToastProgressUpdate { ToastId = "l2", Percentage = 80 });

        var component = Render<ToastRackHost>();
        component.Find("#toastrack-loading").MouseEnter();

        Assert.Equal(2, component.FindAll(".toastrack-toast__progress-circle").Count);
        Assert.Contains("--toastrack-progress: 5%", component.Markup);
        Assert.Contains("--toastrack-progress: 80%", component.Markup);
    }

    [Fact]
    public void Host_MultipleLoadingToasts_RenderCollapsedByDefault()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();

        // Collapsed placeholder shows the aggregate title + count, individual titles are hidden.
        Assert.Contains("Processing...", component.Markup);
        Assert.Contains("toastrack-toast__count", component.Markup);
        Assert.Contains("2", component.Markup);
        Assert.DoesNotContain("First", component.Markup);
        Assert.DoesNotContain("Second", component.Markup);
    }

    [Fact]
    public void Host_CollapsedLoadingTitle_IsCustomizable()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>(parameters =>
            parameters.Add(p => p.CollapsedLoadingTitle, "Wird verarbeitet..."));

        Assert.Contains("Wird verarbeitet...", component.Markup);
        Assert.DoesNotContain("Processing...", component.Markup);
    }

    [Fact]
    public void Host_MultipleLoadingToasts_ExpandOnHover()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();
        component.Find("#toastrack-loading").MouseEnter();

        // Hovering expands the stack to show each loading toast.
        Assert.Contains("First", component.Markup);
        Assert.Contains("Second", component.Markup);
        Assert.DoesNotContain("toastrack-toast__count", component.Markup);
    }

    [Fact]
    public void Host_LoadingHoverOut_CollapsesAgain()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();
        var container = component.Find("#toastrack-loading");
        container.MouseEnter();
        Assert.Contains("First", component.Markup);

        container.MouseLeave();

        Assert.Contains("Processing...", component.Markup);
        Assert.DoesNotContain("First", component.Markup);
    }

    [Fact]
    public void Host_LoadingClick_ExpandsAndRegistersOutsideClickHandler()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();
        component.Find("#toastrack-loading").Click();

        component.WaitForAssertion(() => _module.VerifyInvoke("registerOutsideClick"));
        Assert.Contains("First", component.Markup);
    }

    [Fact]
    public void Host_LoadingClickTwice_UnregistersOutsideClickHandler()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();
        var container = component.Find("#toastrack-loading");

        container.Click();
        component.WaitForAssertion(() => _module.VerifyInvoke("registerOutsideClick"));

        container.Click();
        component.WaitForAssertion(() => _module.VerifyInvoke("unregisterOutsideClick"));
    }

    [Fact]
    public void Host_SingleLoadingToast_ClickDoesNotToggle()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Only" });

        var component = Render<ToastRackHost>();
        component.Find("#toastrack-loading").Click();

        // With a single loading toast, clicking is a no-op — no outside-click handler is registered.
        Assert.DoesNotContain(_module.Invocations, i => i.Identifier == "registerOutsideClick");
    }

    [Fact]
    public void Host_ScrollsTopGroupsToBottomAfterRender()
    {
        var component = Render<ToastRackHost>();

        // Scrolling is triggered by the ToastsUpdated handler, so add the toast after the first render.
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "top", Title = "TopToast", Position = ToastPosition.TopRight });

        component.WaitForAssertion(() =>
            Assert.Contains(
                _module.Invocations,
                i => i.Identifier == "scrollToBottom" && Equals(i.Arguments[0], "toastrackTopRight")));
    }

    [Fact]
    public void Host_WithoutBoundary_DoesNotObserve()
    {
        Render<ToastRackHost>();

        Assert.DoesNotContain(_module.Invocations, i => i.Identifier == "observeBoundary");
    }

    [Fact]
    public void Host_WithBoundarySelector_ObservesBoundary()
    {
        _module.Setup<bool>("observeBoundary", _ => true).SetResult(true);

        var component = Render<ToastRackHost>(parameters =>
            parameters.Add(p => p.BoundarySelector, "#main-content"));

        component.WaitForAssertion(() => _module.VerifyInvoke("observeBoundary"));
    }

    [Fact]
    public void Host_BoundaryChange_AppliesInlinePositionStyles()
    {
        _toastService.ShowSuccessToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Saved",
            Position = ToastPosition.BottomRight,
        });

        var component = Render<ToastRackHost>();

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
    public async Task Host_DisposesWithoutError()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "First" });
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l2", Title = "Second" });

        var component = Render<ToastRackHost>();
        component.Find("#toastrack-loading").Click();

        // Wait for the click to register the handler before disposing.
        component.WaitForAssertion(() => _module.VerifyInvoke("registerOutsideClick"));

        // Disposal unsubscribes from ToastsUpdated and unregisters the outside-click handler.
        await ((IAsyncDisposable)component.Instance).DisposeAsync();

        _module.VerifyInvoke("unregisterOutsideClick");
    }
}
