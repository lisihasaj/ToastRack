namespace ToastRack.Tests;

public class ToastServiceExtensionsTests
{
    private readonly ToastService _service = new();

    [Fact]
    public void Success_ShowsSuccessToastWithTexts()
    {
        _service.Success("Saved", "Your changes were saved.");

        var toast = Assert.Single(_service.Toasts);
        Assert.Equal(ToastVariant.Success, toast.Variant);
        Assert.Equal("Saved", toast.Title);
        Assert.Equal("Your changes were saved.", toast.Caption);
        Assert.Equal(ToastPosition.BottomLeft, toast.Position);
    }

    [Fact]
    public void Warning_ShowsWarningToast()
    {
        _service.Warning("Careful");

        var toast = Assert.Single(_service.Toasts);
        Assert.Equal(ToastVariant.Warning, toast.Variant);
        Assert.Equal("Careful", toast.Title);
        Assert.Null(toast.Caption);
    }

    [Fact]
    public void Error_ShowsErrorToastWithDefaultExpiry()
    {
        _service.Error("Failed");

        var toast = Assert.Single(_service.Toasts);
        Assert.Equal(ToastVariant.Error, toast.Variant);
        Assert.NotEqual(DateTimeOffset.MaxValue, toast.ExpiresAt);
    }

    [Fact]
    public void Info_ShowsInfoToastAtGivenPosition()
    {
        _service.Info("Heads up", position: ToastPosition.TopRight);

        var toast = Assert.Single(_service.Toasts);
        Assert.Equal(ToastVariant.Info, toast.Variant);
        Assert.Equal(ToastPosition.TopRight, toast.Position);
    }

    [Fact]
    public void Loading_ShowsLoadingToast()
    {
        _service.Loading("job", "Uploading...");

        var toast = Assert.Single(_service.LoadingToasts);
        Assert.Equal("job", toast.ToastId);
        Assert.Equal(ToastVariant.Loading, toast.Variant);
        Assert.False(toast.IsProgress);
    }

    [Fact]
    public void Loading_WithProgress_ThenProgress_UpdatesPercentage()
    {
        _service.Loading("job", "Uploading...", showProgress: true);
        _service.Progress("job", 60);

        var toast = Assert.Single(_service.LoadingToasts);
        Assert.True(toast.IsProgress);
        Assert.Equal(60, toast.Percentage);
    }

    [Fact]
    public void Resolve_ReplacesLoadingToastWithResultToast()
    {
        _service.Loading("job", "Uploading...");

        _service.Resolve("job", ToastVariant.Success, "Uploaded");

        Assert.Empty(_service.LoadingToasts);
        var toast = Assert.Single(_service.Toasts);
        Assert.Equal(ToastVariant.Success, toast.Variant);
        Assert.Equal("Uploaded", toast.Title);
    }
}
