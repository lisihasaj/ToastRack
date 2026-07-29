namespace ToastRack.Tests;

public class ToastServiceTests
{
    [Fact]
    public void ShowInfoToast_AddsInfoToastWithTitleAndCaption()
    {
        var service = new ToastService();
        var updateCount = 0;
        service.ToastsUpdated += () => updateCount++;

        service.ShowInfoToast(new ToastOptions
        {
            ToastId = "info-1",
            Title = "Info 1",
            Caption = "Description",
        });

        var toast = Assert.Single(service.Toasts);
        Assert.Equal("info-1", toast.ToastId);
        Assert.Equal(ToastVariant.Info, toast.Variant);
        Assert.Equal("Info 1", toast.Title);
        Assert.Equal("Description", toast.Caption);
        Assert.Equal(1, updateCount);
    }

    [Fact]
    public void ShowErrorToast_AddsNonExpiringErrorToast()
    {
        var service = new ToastService();

        service.ShowErrorToast(new ToastOptions
        {
            ToastId = "error-1",
            Title = "Error 1",
            Caption = "Description",
        });

        var toast = Assert.Single(service.Toasts);
        Assert.Equal(ToastVariant.Error, toast.Variant);
        Assert.Equal(DateTimeOffset.MaxValue, toast.ExpiresAt);
    }

    [Fact]
    public void ShowToast_WithActions_NeverAutoExpires()
    {
        var service = new ToastService();

        service.ShowInfoToast(new ToastOptions
        {
            ToastId = "action-1",
            Title = "Undo?",
            Actions = [new ToastAction { Label = "Undo" }],
        });

        Assert.Equal(DateTimeOffset.MaxValue, service.Toasts[0].ExpiresAt);
    }

    [Fact]
    public void ShowToast_IgnoresDuplicateToastId()
    {
        var service = new ToastService();

        service.ShowInfoToast(new ToastOptions { ToastId = "info-1", Title = "Info 1" });
        service.ShowInfoToast(new ToastOptions { ToastId = "info-1", Title = "Info 1 again" });

        var toast = Assert.Single(service.Toasts);
        Assert.Equal("Info 1", toast.Title);
    }

    [Fact]
    public void ShowToast_AutoGeneratesToastIdWhenMissing()
    {
        var service = new ToastService();

        service.ShowSuccessToast(new ToastOptions { Title = "One" });
        service.ShowSuccessToast(new ToastOptions { Title = "Two" });

        Assert.Equal(2, service.Toasts.Count);
        Assert.NotNull(service.Toasts[0].ToastId);
        Assert.NotEqual(service.Toasts[0].ToastId, service.Toasts[1].ToastId);
    }

    [Fact]
    public void ToastsByPosition_GroupsAndReversesBottomGroups()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "t1", Position = ToastPosition.TopLeft });
        service.ShowInfoToast(new ToastOptions { ToastId = "t2", Position = ToastPosition.TopLeft });
        service.ShowInfoToast(new ToastOptions { ToastId = "b1", Position = ToastPosition.BottomLeft });
        service.ShowInfoToast(new ToastOptions { ToastId = "b2", Position = ToastPosition.BottomLeft });

        var groups = service.ToastsByPosition;

        // Top groups keep insertion order; bottom groups are newest-first.
        Assert.Equal(["t1", "t2"], groups[ToastPosition.TopLeft].Select(t => t.ToastId));
        Assert.Equal(["b2", "b1"], groups[ToastPosition.BottomLeft].Select(t => t.ToastId));
    }

    [Fact]
    public void ShowLoadingToast_AddsLoadingToastToLoadingList()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Title = "File is loading",
            Caption = "Description",
        });

        var toast = Assert.Single(service.LoadingToasts);
        Assert.Empty(service.Toasts);
        Assert.Equal("loading-1", toast.ToastId);
        Assert.Equal(ToastVariant.Loading, toast.Variant);
        Assert.Equal("File is loading", toast.Title);
    }

    [Fact]
    public void ShowLoadingToast_IgnoresDuplicateToastId()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", Title = "First" });
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", Title = "Second" });

        var toast = Assert.Single(service.LoadingToasts);
        Assert.Equal("First", toast.Title);
    }

    [Fact]
    public void ShowLoadingToast_WithProgress_SeedsInitialPercentage()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Title = "Exporting",
            IsProgress = true,
        });

        var toast = service.LoadingToasts[0];
        Assert.True(toast.IsProgress);
        Assert.Equal(5, toast.Percentage);
    }

    [Fact]
    public void ShowLoadingToast_WithoutProgress_HasNoPercentage()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", Title = "Exporting" });

        var toast = service.LoadingToasts[0];
        Assert.False(toast.IsProgress);
        Assert.Equal(0, toast.Percentage);
    }

    [Fact]
    public void UpdateLoadingToastProgress_SetsPercentageAndNotifies()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        var updateCount = 0;
        service.ToastsUpdated += () => updateCount++;

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-1",
            Percentage = 42,
        });

        Assert.Equal(42, service.LoadingToasts[0].Percentage);
        Assert.Equal(1, updateCount);
    }

    [Fact]
    public void UpdateLoadingToastProgress_ClampsAboveHundredToHundred()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-1",
            Percentage = 250,
        });

        Assert.Equal(100, service.LoadingToasts[0].Percentage);
    }

    [Fact]
    public void UpdateLoadingToastProgress_ClampsBelowInitialToInitialPercentage()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        // The progress circle never drops below its initial fill, so 0 and negative
        // values are raised to the initial percentage rather than clamped to 0.
        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-1",
            Percentage = -10,
        });

        Assert.Equal(5, service.LoadingToasts[0].Percentage);

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-1",
            Percentage = 0,
        });

        Assert.Equal(5, service.LoadingToasts[0].Percentage);
    }

    [Fact]
    public void UpdateLoadingToastProgress_IgnoresUnknownToastId()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        var updateCount = 0;
        service.ToastsUpdated += () => updateCount++;

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "does-not-exist",
            Percentage = 80,
        });

        Assert.Equal(5, service.LoadingToasts[0].Percentage);
        Assert.Equal(0, updateCount);
    }

    [Fact]
    public void UpdateLoadingToastProgress_IgnoresNullToastId()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = null,
            Percentage = 80,
        });

        Assert.Equal(5, service.LoadingToasts[0].Percentage);
    }

    [Fact]
    public void UpdateLoadingToastProgress_UpdatesOnlyTargetedToast()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-2", IsProgress = true });

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-2",
            Percentage = 70,
        });

        Assert.Equal(5, service.LoadingToasts[0].Percentage);
        Assert.Equal(70, service.LoadingToasts[1].Percentage);
    }

    [Fact]
    public void ResolveLoadingToast_RemovesProgressToast()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });
        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "loading-1",
            Percentage = 100,
        });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Empty(service.LoadingToasts);
        var toast = Assert.Single(service.Toasts);
        Assert.False(toast.IsProgress);
    }

    [Fact]
    public void ResolveLoadingToast_ReplacesLoadingToastWithResolvedVariant()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", Title = "File is loading" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
            Title = "Done",
            Caption = "Completed",
        });

        Assert.Empty(service.LoadingToasts);
        var toast = Assert.Single(service.Toasts);
        Assert.Equal(ToastVariant.Success, toast.Variant);
        Assert.Equal("Done", toast.Title);
        Assert.Equal("Completed", toast.Caption);
    }

    [Fact]
    public void ResolveLoadingToast_UsesDefaultTitleAndCaptionWhenNotProvided()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", Title = "File is loading" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        var toast = Assert.Single(service.Toasts);
        Assert.Equal("Success", toast.Title);
        Assert.Equal("Task is completed successfully.", toast.Caption);
    }

    [Fact]
    public void ResolveLoadingToast_HonorsPositionOption()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
            Position = ToastPosition.TopRight,
        });

        Assert.Equal(ToastPosition.TopRight, service.Toasts[0].Position);
    }

    [Fact]
    public void ResolveLoadingToast_IgnoresUnknownToastId()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        service.ResolveLoadingToast(new ResolveToastOptions { ToastId = "unknown" });

        Assert.Single(service.LoadingToasts);
        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void RemoveToast_RemovesToastById()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "info-1", Title = "Info 1" });

        service.RemoveToast("info-1");

        Assert.Empty(service.Toasts);
    }

    [Fact]
    public void RemoveToast_RemovesLoadingToastById()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1", IsProgress = true });

        service.RemoveToast("loading-1");

        Assert.Empty(service.LoadingToasts);
    }

    [Fact]
    public void RemoveToast_RemovesToastByInstance()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "info-1" });
        var toast = service.Toasts[0];

        service.RemoveToast(toast);

        Assert.Empty(service.Toasts);
    }

    [Fact]
    public async Task ShowToast_IsThreadSafeUnderConcurrentUse()
    {
        var service = new ToastService();

        await Task.WhenAll(Enumerable.Range(0, 8).Select(worker => Task.Run(() =>
        {
            for (var i = 0; i < 250; i++)
            {
                service.ShowInfoToast(new ToastOptions { ToastId = $"toast-{worker}-{i}" });
                if (i % 2 == 0)
                {
                    service.RemoveToast($"toast-{worker}-{i}");
                }
            }
        })));

        // 8 workers × 250 toasts, half removed again.
        Assert.Equal(8 * 125, service.Toasts.Count);
    }
}
