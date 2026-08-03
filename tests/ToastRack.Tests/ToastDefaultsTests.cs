using Bunit.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using ToastRackComponent = ToastRack.Components.ToastRack;

namespace ToastRack.Tests;

public class ToastDefaultsTests : BunitContext
{
    private readonly ToastService _toastService = new();

    public ToastDefaultsTests()
    {
        Services.AddSingleton<IToastService>(_toastService);
        var module = JSInterop.SetupModule("./_content/ToastRack/toastrack.js");
        module.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void ServiceDefaults_AreLibraryDefaults_BeforeAnyRackRenders()
    {
        Assert.Equal(ToastPosition.BottomLeft, _toastService.Defaults.Position);
        Assert.True(_toastService.Defaults.CloseByClick);
        Assert.Equal(5, _toastService.Defaults.Expiry);
    }

    [Fact]
    public void RackWithoutParameters_LeavesLibraryDefaultsInPlace()
    {
        Render<ToastRackComponent>();

        _toastService.ShowInfoToast(new ToastOptions { ToastId = "t1", Title = "Bare" });

        var toast = _toastService.Toasts[0];
        Assert.Equal(ToastPosition.BottomLeft, toast.Position);
        Assert.True(toast.CloseByClick);
        Assert.Equal(5, toast.Expiry);
    }

    [Fact]
    public void RackParameters_BecomeTheDefaultsForBareOptions()
    {
        Render<ToastRackComponent>(p => p
            .Add(c => c.Position, ToastPosition.TopRight)
            .Add(c => c.CloseByClick, false)
            .Add(c => c.Expiry, 12));

        _toastService.ShowInfoToast(new ToastOptions { ToastId = "t1", Title = "Bare" });

        var toast = _toastService.Toasts[0];
        Assert.Equal(ToastPosition.TopRight, toast.Position);
        Assert.False(toast.CloseByClick);
        Assert.Equal(12, toast.Expiry);
    }

    [Fact]
    public void ExplicitToastOptions_WinOverRackDefaults()
    {
        Render<ToastRackComponent>(p => p
            .Add(c => c.Position, ToastPosition.TopRight)
            .Add(c => c.CloseByClick, false)
            .Add(c => c.Expiry, 12));

        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Explicit",
            Position = ToastPosition.BottomCenter,
            CloseByClick = true,
            Expiry = 3,
        });

        var toast = _toastService.Toasts[0];
        Assert.Equal(ToastPosition.BottomCenter, toast.Position);
        Assert.True(toast.CloseByClick);
        Assert.Equal(3, toast.Expiry);
    }

    [Fact]
    public void RackDefaultPosition_DrivesRenderedGroupAndAnimation()
    {
        var component = Render<ToastRackComponent>(p => p
            .Add(c => c.Position, ToastPosition.TopCenter));

        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "t1", Title = "Saved" });

        Assert.Contains("toastrack--top-center", component.Markup);
        Assert.Contains("id=\"toastrackTopCenter\"", component.Markup);
        Assert.Contains("toastrack-animate-slide-down", component.Markup);
    }

    [Fact]
    public void RackDefaultCloseByClick_False_KeepsToastOnBodyClick()
    {
        var component = Render<ToastRackComponent>(p => p
            .Add(c => c.CloseByClick, false));

        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "t1", Title = "Sticky" });

        component.Find(".toastrack-toast").Click();

        Assert.Single(_toastService.Toasts);
    }

    [Fact]
    public void RackDefaultCloseByClick_IsOverriddenByExplicitOption()
    {
        var component = Render<ToastRackComponent>(p => p
            .Add(c => c.CloseByClick, false));

        _toastService.ShowSuccessToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Clickable",
            CloseByClick = true,
        });

        component.Find(".toastrack-toast").Click();

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public void LoadingToast_UsesRackDefaultPosition()
    {
        Render<ToastRackComponent>(p => p.Add(c => c.Position, ToastPosition.TopLeft));

        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Title = "Working" });

        Assert.Equal(ToastPosition.TopLeft, _toastService.Toasts[0].Position);
    }

    [Fact]
    public void ExtensionShorthands_UseRackDefaultPosition()
    {
        Render<ToastRackComponent>(p => p.Add(c => c.Position, ToastPosition.TopRight));

        _toastService.Success("Saved");
        _toastService.Loading("job", "Uploading...");

        Assert.All(_toastService.Toasts, t => Assert.Equal(ToastPosition.TopRight, t.Position));
    }

    [Fact]
    public void ExtensionShorthands_HonorExplicitPositionOverRackDefault()
    {
        Render<ToastRackComponent>(p => p.Add(c => c.Position, ToastPosition.TopRight));

        _toastService.Info("Heads up", position: ToastPosition.BottomCenter);

        Assert.Equal(ToastPosition.BottomCenter, _toastService.Toasts[0].Position);
    }

    [Fact]
    public void ToastWithActions_NeverExpires_EvenWithRackDefaultExpiry()
    {
        Render<ToastRackComponent>(p => p.Add(c => c.Expiry, 12));

        _toastService.ShowInfoToast(new ToastOptions
        {
            ToastId = "t1",
            Title = "Undo?",
            Actions = [new ToastAction { Label = "Undo" }],
        });

        var toast = _toastService.Toasts[0];
        Assert.Null(toast.Expiry);
        Assert.Equal(DateTimeOffset.MaxValue, toast.ExpiresAt);
    }

    [Fact]
    public void ChangedRackParameters_ApplyToSubsequentToasts()
    {
        var component = Render<ToastRackComponent>(p => p
            .Add(c => c.Position, ToastPosition.TopLeft));

        _toastService.ShowInfoToast(new ToastOptions { ToastId = "first", Title = "First" });

        component.Render(p => p.Add(c => c.Position, ToastPosition.BottomRight));

        _toastService.ShowInfoToast(new ToastOptions { ToastId = "second", Title = "Second" });

        Assert.Equal(ToastPosition.TopLeft, _toastService.Toasts[0].Position);
        Assert.Equal(ToastPosition.BottomRight, _toastService.Toasts[1].Position);
    }

    [Fact]
    public void SetDefaults_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => _toastService.SetDefaults(null!));
    }
}
