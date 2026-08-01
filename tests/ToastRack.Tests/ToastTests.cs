using Microsoft.Extensions.DependencyInjection;
using ToastRack.Components;

namespace ToastRack.Tests;

public class ToastTests : BunitContext
{
    private readonly ToastService _toastService = new();

    public ToastTests()
    {
        Services.AddSingleton<IToastService>(_toastService);
    }

    private IRenderedComponent<Toast> RenderToast(ToastItem item, string? animationClass = null)
    {
        return Render<Toast>(parameters =>
        {
            parameters.Add(p => p.Item, item);
            if (animationClass is not null)
            {
                parameters.Add(p => p.AnimationClass, animationClass);
            }
        });
    }

    private static ToastItem NonExpiringItem(ToastVariant variant, string? title = null, string? caption = null)
    {
        return new ToastItem
        {
            ToastId = "toast-1",
            Variant = variant,
            Title = title,
            Caption = caption,
            ExpiresAt = DateTimeOffset.MaxValue,
        };
    }

    [Fact]
    public void Toast_RendersTitleCaptionAndVariantClass()
    {
        var item = NonExpiringItem(ToastVariant.Success, "Saved", "Your changes were saved.");

        var component = RenderToast(item, "toastrack-animate-slide-down");

        Assert.Contains("Saved", component.Markup);
        Assert.Contains("Your changes were saved.", component.Markup);
        Assert.Contains("toastrack-toast--success", component.Markup);
        Assert.Contains("toastrack-toast--defined", component.Markup);
        Assert.Contains("toastrack-animate-slide-down", component.Markup);
    }

    [Fact]
    public void Toast_ErrorVariant_UsesAlertRole()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Error, "Failed"));

        Assert.Equal("alert", component.Find(".toastrack-toast").GetAttribute("role"));
    }

    [Fact]
    public void Toast_NonErrorVariant_UsesStatusRole()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Success, "Saved"));

        Assert.Equal("status", component.Find(".toastrack-toast").GetAttribute("role"));
    }

    [Fact]
    public void Toast_RendersDefaultVariantIconAsSvg()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Success, "Saved"));

        Assert.NotNull(component.Find(".toastrack-toast__icon svg"));
    }

    [Fact]
    public void Toast_UsesCustomIconWhenProvided()
    {
        var item = NonExpiringItem(ToastVariant.Info, "Custom");
        item.Icon = builder => builder.AddMarkupContent(0, "<i class=\"my-icon-class\">star</i>");

        var component = RenderToast(item);

        Assert.Contains("my-icon-class", component.Markup);
        Assert.Contains("star", component.Markup);
        // The built-in svg icon should not appear when a custom icon is supplied.
        Assert.Empty(component.FindAll(".toastrack-toast__icon svg"));
    }

    [Fact]
    public void Toast_LoadingVariant_RendersSpinnerAndTitleWithoutCloseButton()
    {
        var item = NonExpiringItem(ToastVariant.Loading, "Processing...");

        var component = RenderToast(item);

        Assert.Contains("toastrack-spinner", component.Markup);
        Assert.Contains("Processing...", component.Markup);
        Assert.Contains("toastrack-toast--loading", component.Markup);
        // Loading toasts have no icon/close section.
        Assert.DoesNotContain("toastrack-toast__close", component.Markup);
    }

    [Fact]
    public void Toast_ProgressVariant_RendersProgressCircleInsteadOfSpinner()
    {
        var item = NonExpiringItem(ToastVariant.Loading, "Exporting...");
        item.IsProgress = true;
        item.Percentage = 40;

        var component = RenderToast(item);

        Assert.Contains("toastrack-toast__progress-circle", component.Markup);
        Assert.DoesNotContain("toastrack-spinner", component.Markup);
        Assert.Contains("Exporting...", component.Markup);
    }

    [Fact]
    public void Toast_ProgressVariant_SetsProgressCustomPropertyAndAriaAttributes()
    {
        var item = NonExpiringItem(ToastVariant.Loading, "Exporting...");
        item.IsProgress = true;
        item.Percentage = 40;

        var component = RenderToast(item);
        var circle = component.Find(".toastrack-toast__progress-circle");

        Assert.Contains("--toastrack-progress: 40%", component.Markup);
        Assert.Equal("progressbar", circle.GetAttribute("role"));
        Assert.Equal("40", circle.GetAttribute("aria-valuenow"));
        Assert.Equal("0", circle.GetAttribute("aria-valuemin"));
        Assert.Equal("100", circle.GetAttribute("aria-valuemax"));
    }

    [Fact]
    public void Toast_ProgressVariant_ClampsOutOfRangePercentage()
    {
        var item = NonExpiringItem(ToastVariant.Loading, "Exporting...");
        item.IsProgress = true;
        item.Percentage = 150;

        var component = RenderToast(item);

        Assert.Contains("--toastrack-progress: 100%", component.Markup);
        Assert.Equal("100", component.Find(".toastrack-toast__progress-circle").GetAttribute("aria-valuenow"));

        // The circle clamps to 0..100 only. The 5% floor is applied by ToastService when
        // it seeds and updates the toast, not by the component.
        item.Percentage = -20;
        component.Render();

        Assert.Contains("--toastrack-progress: 0%", component.Markup);
        Assert.Equal("0", component.Find(".toastrack-toast__progress-circle").GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void Toast_ProgressVariant_UpdatesCircleWhenPercentageChanges()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "progress-1",
            Title = "Exporting...",
            IsProgress = true,
        });
        var item = _toastService.LoadingToasts.Single();

        var component = RenderToast(item);
        Assert.Contains("--toastrack-progress: 5%", component.Markup);

        _toastService.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "progress-1",
            Percentage = 75,
        });
        component.Render();

        Assert.Contains("--toastrack-progress: 75%", component.Markup);
        Assert.Equal("75", component.Find(".toastrack-toast__progress-circle").GetAttribute("aria-valuenow"));
    }

    [Fact]
    public void Toast_ProgressVariant_DoesNotCloseOnBodyClick()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "progress-1",
            Title = "Exporting...",
            IsProgress = true,
        });
        var item = _toastService.LoadingToasts.Single();

        var component = RenderToast(item);
        component.Find(".toastrack-toast").Click();

        Assert.Single(_toastService.LoadingToasts);
    }

    [Fact]
    public void Toast_LoadingVariant_RendersCaptionWhenProvided()
    {
        var item = NonExpiringItem(ToastVariant.Loading, "Uploading...", "3 of 7 files done.");

        var component = RenderToast(item);

        Assert.Contains("toastrack-toast__content", component.Markup);
        Assert.Contains("Uploading...", component.Markup);
        Assert.Contains("3 of 7 files done.", component.Markup);
    }

    [Fact]
    public void Toast_CustomVariant_RendersFragment()
    {
        var item = new ToastItem
        {
            ToastId = "custom-1",
            Variant = ToastVariant.Custom,
            ExpiresAt = DateTimeOffset.MaxValue,
            Fragment = builder => builder.AddMarkupContent(0, "<p class=\"my-custom-content\">Hello</p>"),
        };

        var component = RenderToast(item);

        Assert.Contains("my-custom-content", component.Markup);
        Assert.Contains("Hello", component.Markup);
        Assert.Contains("toastrack-toast--custom", component.Markup);
    }

    [Fact]
    public void Toast_WithoutTitleOrCaption_OmitsContentBlock()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Info));

        Assert.DoesNotContain("toastrack-toast__content", component.Markup);
    }

    [Fact]
    public void Toast_RendersActionButtons()
    {
        var item = NonExpiringItem(ToastVariant.Info, "Undo?", "You deleted an item.");
        item.Actions =
        [
            new ToastAction { Id = "undo-action", Label = "Undo" },
        ];

        var component = RenderToast(item);

        Assert.Contains("toastrack-toast__actions", component.Markup);
        Assert.Contains("Undo", component.Markup);
    }

    [Fact]
    public void Toast_ActionClick_InvokesSyncHandlerAndRemovesToast()
    {
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "act-toast", Title = "Undo?" });
        var item = _toastService.Toasts.Single();

        var clicked = false;
        item.Actions =
        [
            new ToastAction { Id = "undo-action", Label = "Undo", OnClick = () => clicked = true },
        ];

        var component = RenderToast(item);
        component.Find("#undo-action").Click();

        Assert.True(clicked);
        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public async Task Toast_ActionClick_InvokesAsyncHandlerAndRemovesToast()
    {
        _toastService.ShowInfoToast(new ToastOptions { ToastId = "act-toast-async", Title = "Retry?" });
        var item = _toastService.Toasts.Single();

        var tcs = new TaskCompletionSource();
        item.Actions =
        [
            new ToastAction
            {
                Id = "retry-action",
                Label = "Retry",
                OnClickAsync = () =>
                {
                    tcs.SetResult();
                    return Task.CompletedTask;
                },
            },
        ];

        var component = RenderToast(item);
        component.Find("#retry-action").Click();

        await tcs.Task;
        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public void Toast_DisabledAction_RendersDisabledButton()
    {
        var item = NonExpiringItem(ToastVariant.Info, "Undo?");
        item.Actions =
        [
            new ToastAction { Id = "undo-action", Label = "Undo", IsDisabled = true },
        ];

        var component = RenderToast(item);

        Assert.True(component.Find("#undo-action").HasAttribute("disabled"));
    }

    [Fact]
    public void Toast_BodyClick_RemovesToastWhenCloseByClickEnabled()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "body-close", Title = "Done", CloseByClick = true });
        var item = _toastService.Toasts.Single();

        var component = RenderToast(item);
        component.Find(".toastrack-toast").Click();

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public void Toast_BodyClick_DoesNotRemoveToastWhenCloseByClickDisabled()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "body-noclose", Title = "Done", CloseByClick = false });
        var item = _toastService.Toasts.Single();

        var component = RenderToast(item);
        component.Find(".toastrack-toast").Click();

        Assert.Single(_toastService.Toasts);
    }

    [Fact]
    public void Toast_CloseButton_RemovesToast()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "close-btn", Title = "Done", CloseByClick = false });
        var item = _toastService.Toasts.Single();

        var component = RenderToast(item);
        component.Find(".toastrack-toast__close").Click();

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public void Toast_ExpiresImmediately_RemovesToastOnInit()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "expired", Title = "Old", Expiry = 5 });
        var item = _toastService.Toasts.Single();
        // Force the toast to be already expired at render time.
        item.ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1);

        RenderToast(item);

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public async Task Toast_AutoExpires_RemovesToastAfterDelay()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "auto-expire", Title = "Bye", Expiry = 5 });
        var item = _toastService.Toasts.Single();
        item.ExpiresAt = DateTimeOffset.UtcNow.AddMilliseconds(50);

        RenderToast(item);

        // The expiry timer fires on a background thread and removes the toast from the service.
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (_toastService.Toasts.Count > 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public async Task Toast_ResolvedLoadingToast_AutoExpires()
    {
        _toastService.ShowLoadingToast(new LoadingToastOptions { ToastId = "resolve-expire", Title = "Working..." });
        var loadingItem = _toastService.LoadingToasts.Single();

        var component = RenderToast(loadingItem);

        // Resolving replaces the loading item with a new one that keeps the same ToastId, so the
        // keyed Toast instance is reused and only receives the resolved item as a parameter.
        _toastService.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "resolve-expire",
            ReplaceWith = ToastVariant.Success,
        });
        var resolvedItem = _toastService.Toasts.Single();
        resolvedItem.ExpiresAt = DateTimeOffset.UtcNow.AddMilliseconds(50);

        component.Render(parameters => parameters.Add(p => p.Item, resolvedItem));

        // The reused instance must re-arm its expiry timer for the new item without any hover.
        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (_toastService.Toasts.Count > 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public async Task Toast_MouseEnter_PausesExpiry()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "pause", Title = "Hover me", Expiry = 5 });
        var item = _toastService.Toasts.Single();
        item.ExpiresAt = DateTimeOffset.UtcNow.AddMilliseconds(80);

        var component = RenderToast(item);
        component.Find(".toastrack-toast").MouseEnter();

        // While hovered, the expiry timer is paused, so the toast survives past its original expiry.
        await Task.Delay(200);
        Assert.Single(_toastService.Toasts);
    }

    [Fact]
    public async Task Toast_MouseLeave_ResumesExpiryWithRemainingTime()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "resume", Title = "Hover me", Expiry = 5 });
        var item = _toastService.Toasts.Single();
        item.ExpiresAt = DateTimeOffset.UtcNow.AddMilliseconds(150);

        var component = RenderToast(item);
        var toast = component.Find(".toastrack-toast");
        toast.MouseEnter();

        // Hover past the original expiry: the toast must survive while paused.
        await Task.Delay(300);
        Assert.Single(_toastService.Toasts);

        // Leaving restarts the timer with the remaining time captured on hover (~150 ms).
        toast.MouseLeave();

        var deadline = DateTime.UtcNow.AddSeconds(2);
        while (_toastService.Toasts.Count > 0 && DateTime.UtcNow < deadline)
        {
            await Task.Delay(25);
        }

        Assert.Empty(_toastService.Toasts);
    }

    [Fact]
    public void Toast_NonExpiring_HoverEventsAreNoOps()
    {
        var item = NonExpiringItem(ToastVariant.Info, "Sticky");

        var component = RenderToast(item);
        var toast = component.Find(".toastrack-toast");
        toast.MouseEnter();
        toast.MouseLeave();

        Assert.Equal(DateTimeOffset.MaxValue, item.ExpiresAt);
    }

    [Fact]
    public void Toast_MouseLeaveWithoutEnter_DoesNotChangeExpiry()
    {
        _toastService.ShowSuccessToast(new ToastOptions { ToastId = "leave-only", Title = "Bye", Expiry = 60 });
        var item = _toastService.Toasts.Single();
        var originalExpiresAt = item.ExpiresAt;

        var component = RenderToast(item);
        component.Find(".toastrack-toast").MouseLeave();

        Assert.Equal(originalExpiresAt, item.ExpiresAt);
        Assert.Single(_toastService.Toasts);
    }

    [Fact]
    public void Toast_WarningVariant_RendersWarningClassAndBuiltInIcon()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Warning, "Careful"));

        Assert.Contains("toastrack-toast--warning", component.Markup);
        Assert.Contains("toastrack-toast--defined", component.Markup);
        Assert.NotNull(component.Find(".toastrack-toast__icon svg"));
    }

    [Fact]
    public void Toast_UnknownVariant_FallsBackToInfoStyling()
    {
        var component = RenderToast(NonExpiringItem((ToastVariant)999, "Odd"));

        Assert.Contains("toastrack-toast--info", component.Markup);
        Assert.Contains("toastrack-toast--defined", component.Markup);
        Assert.NotNull(component.Find(".toastrack-toast__icon svg"));
    }

    [Theory]
    [InlineData(ToastActionStyle.Primary, "toastrack-action--primary")]
    [InlineData(ToastActionStyle.Outline, "toastrack-action--outline")]
    [InlineData(ToastActionStyle.Text, "toastrack-action--text")]
    public void Toast_ActionStyle_MapsToCssClass(ToastActionStyle style, string expectedClass)
    {
        var item = NonExpiringItem(ToastVariant.Info, "Undo?");
        item.Actions =
        [
            new ToastAction { Id = "styled-action", Label = "Undo", Style = style },
        ];

        var component = RenderToast(item);

        Assert.Contains(expectedClass, component.Find("#styled-action").ClassList);
    }

    [Fact]
    public void Toast_ActionCustomCssClass_IsAppended()
    {
        var item = NonExpiringItem(ToastVariant.Info, "Undo?");
        item.Actions =
        [
            new ToastAction { Id = "custom-action", Label = "Undo", CssClass = "my-action" },
        ];

        var component = RenderToast(item);
        var button = component.Find("#custom-action");

        Assert.Contains("toastrack-action--primary", button.ClassList);
        Assert.Contains("my-action", button.ClassList);
    }

    [Fact]
    public void Toast_CaptionOnly_RendersContentWithoutTitle()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Success, caption: "Just a caption"));

        Assert.Contains("toastrack-toast__caption", component.Markup);
        Assert.DoesNotContain("toastrack-toast__title", component.Markup);
    }

    [Fact]
    public void Toast_LoadingVariant_CaptionOnly_RendersContentWithoutTitle()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Loading, caption: "3 of 7 files done."));

        Assert.Contains("toastrack-toast__caption", component.Markup);
        Assert.DoesNotContain("toastrack-toast__title", component.Markup);
    }

    [Fact]
    public void Toast_LoadingVariant_WithoutTexts_OmitsContentBlock()
    {
        var component = RenderToast(NonExpiringItem(ToastVariant.Loading));

        Assert.Contains("toastrack-spinner", component.Markup);
        Assert.DoesNotContain("toastrack-toast__content", component.Markup);
    }
}
