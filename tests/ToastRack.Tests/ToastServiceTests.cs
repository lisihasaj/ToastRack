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
    public void ShowErrorToast_AddsErrorToastWithDefaultExpiry()
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
        Assert.NotEqual(DateTimeOffset.MaxValue, toast.ExpiresAt);
        Assert.True(toast.ExpiresAt <= DateTimeOffset.UtcNow.AddSeconds(5));
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
    public void ShowLoadingToast_AddsLoadingToastToMainList()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Title = "File is loading",
            Caption = "Description",
        });

        // Loading toasts live in the same list as every other variant.
        var toast = Assert.Single(service.Toasts);
        Assert.Same(toast, Assert.Single(service.LoadingToasts));
        Assert.Equal("loading-1", toast.ToastId);
        Assert.Equal(ToastVariant.Loading, toast.Variant);
        Assert.Equal("File is loading", toast.Title);
        Assert.Equal(ToastPosition.BottomLeft, toast.Position);
    }

    [Fact]
    public void ShowLoadingToast_HonorsPosition()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Title = "Uploading",
            Position = ToastPosition.TopCenter,
        });

        Assert.Equal(ToastPosition.TopCenter, service.Toasts[0].Position);
        Assert.Contains(ToastPosition.TopCenter, service.ToastsByPosition.Keys);
    }

    [Fact]
    public void ShowLoadingToast_StacksWithOtherVariantsInInsertionOrder()
    {
        var service = new ToastService();

        service.ShowInfoToast(new ToastOptions { ToastId = "i1", Position = ToastPosition.TopRight });
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "l1", Position = ToastPosition.TopRight });
        service.ShowSuccessToast(new ToastOptions { ToastId = "s1", Position = ToastPosition.TopRight });

        Assert.Equal(
            ["i1", "l1", "s1"],
            service.ToastsByPosition[ToastPosition.TopRight].Select(t => t.ToastId));
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
    public void ResolveLoadingToast_ReplacesLoadingToastWithResolvedVariant()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Title = "File is loading",
            IsProgress = true,
        });

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
        Assert.False(toast.IsProgress);
        Assert.Equal(0, toast.Percentage);
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
    public void ResolveLoadingToast_KeepsLoadingToastPositionWhenNotProvided()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions
        {
            ToastId = "loading-1",
            Position = ToastPosition.TopCenter,
        });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Equal(ToastPosition.TopCenter, service.Toasts[0].Position);
    }

    [Fact]
    public void ResolveLoadingToast_KeepsToastAtItsPlaceInTheStack()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "first" });
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });
        service.ShowInfoToast(new ToastOptions { ToastId = "third" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Equal(["first", "loading-1", "third"], service.Toasts.Select(t => t.ToastId));
    }

    [Fact]
    public void ResolveLoadingToast_PreservesTheSameToastInstance()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });
        var original = service.Toasts[0];

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Same(original, service.Toasts[0]);
    }

    [Fact]
    public void ResolveLoadingToast_RaisesToastsUpdatedExactlyOnce()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        var updates = 0;
        service.ToastsUpdated += () => updates++;

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Equal(1, updates);
    }

    [Fact]
    public void ResolveLoadingToast_DoesNotNotifyWhenNoMatchingLoadingToastExists()
    {
        var service = new ToastService();

        var updates = 0;
        service.ToastsUpdated += () => updates++;

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "missing",
            ReplaceWith = ToastVariant.Success,
        });

        Assert.Equal(0, updates);
    }

    [Fact]
    public void ResolveLoadingToast_SetsExpirySoTheResolvedToastAutoCloses()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });
        Assert.Equal(DateTimeOffset.MaxValue, service.Toasts[0].ExpiresAt);

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = ToastVariant.Success,
            Expiry = 4,
        });

        var toast = service.Toasts[0];
        Assert.Equal(4, toast.Expiry);
        Assert.NotEqual(DateTimeOffset.MaxValue, toast.ExpiresAt);
    }

    [Fact]
    public void ResolveLoadingToast_IgnoresUnknownToastId()
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        service.ResolveLoadingToast(new ResolveToastOptions { ToastId = "unknown" });

        // The loading toast is untouched and nothing was resolved.
        var toast = Assert.Single(service.Toasts);
        Assert.Equal(ToastVariant.Loading, toast.Variant);
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
    public void ShowLoadingToast_AutoGeneratesToastIdWhenMissing()
    {
        var service = new ToastService();

        service.ShowLoadingToast(new LoadingToastOptions { Title = "One" });
        service.ShowLoadingToast(new LoadingToastOptions { Title = "Two" });

        Assert.Equal(2, service.LoadingToasts.Count);
        Assert.NotNull(service.LoadingToasts[0].ToastId);
        Assert.NotEqual(service.LoadingToasts[0].ToastId, service.LoadingToasts[1].ToastId);
    }

    [Fact]
    public void UpdateLoadingToastProgress_IgnoresNonLoadingToastWithSameId()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "not-loading" });

        service.UpdateLoadingToastProgress(new ToastProgressUpdate
        {
            ToastId = "not-loading",
            Percentage = 80,
        });

        Assert.Equal(0, service.Toasts[0].Percentage);
    }

    [Theory]
    [InlineData(ToastVariant.Error, "Error", "An error occurred.")]
    [InlineData(ToastVariant.Warning, "Warning", "Task completed with warning.")]
    [InlineData(ToastVariant.Info, "Information", "Task is completed.")]
    public void ResolveLoadingToast_UsesVariantDefaultTexts(ToastVariant variant, string title, string caption)
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = variant,
        });

        Assert.Empty(service.LoadingToasts);
        var toast = Assert.Single(service.Toasts);
        Assert.Equal(variant, toast.Variant);
        Assert.Equal(title, toast.Title);
        Assert.Equal(caption, toast.Caption);
    }

    [Theory]
    [InlineData(ToastVariant.Error)]
    [InlineData(ToastVariant.Warning)]
    [InlineData(ToastVariant.Info)]
    public void ResolveLoadingToast_HonorsCustomTextsForEachVariant(ToastVariant variant)
    {
        var service = new ToastService();
        service.ShowLoadingToast(new LoadingToastOptions { ToastId = "loading-1" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "loading-1",
            ReplaceWith = variant,
            Title = "Custom title",
            Caption = "Custom caption",
        });

        var toast = Assert.Single(service.Toasts);
        Assert.Equal("Custom title", toast.Title);
        Assert.Equal("Custom caption", toast.Caption);
    }

    [Fact]
    public void ResolveLoadingToast_IgnoresNonLoadingToastWithSameId()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "not-loading", Title = "Info" });

        service.ResolveLoadingToast(new ResolveToastOptions
        {
            ToastId = "not-loading",
            ReplaceWith = ToastVariant.Success,
        });

        // The info toast is not a loading toast, so nothing is resolved.
        var toast = Assert.Single(service.Toasts);
        Assert.Equal(ToastVariant.Info, toast.Variant);
        Assert.Equal("Info", toast.Title);
    }

    [Fact]
    public void RemoveToast_IgnoresUnknownToastIdWithoutNotifying()
    {
        var service = new ToastService();
        service.ShowInfoToast(new ToastOptions { ToastId = "info-1" });

        var updateCount = 0;
        service.ToastsUpdated += () => updateCount++;

        service.RemoveToast("does-not-exist");

        Assert.Single(service.Toasts);
        Assert.Equal(0, updateCount);
    }

    [Fact]
    public void Methods_ThrowOnNullArguments()
    {
        var service = new ToastService();

        Assert.Throws<ArgumentNullException>(() => service.ShowToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.ShowSuccessToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.ShowWarningToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.ShowErrorToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.ShowInfoToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.ShowLoadingToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.UpdateLoadingToastProgress(null!));
        Assert.Throws<ArgumentNullException>(() => service.ResolveLoadingToast(null!));
        Assert.Throws<ArgumentNullException>(() => service.RemoveToast((ToastItem)null!));
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
