using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.ViewModels;

internal sealed partial class BlogViewModel : RoutableViewModelBase
{
    private readonly NitroxBlogService? nitroxBlogService;
    public static Bitmap FallbackImage { get; } = AssetHelper.GetAssetFromStream("/Assets/Images/blog/vines.png", static stream => new Bitmap(stream));

    [ObservableProperty]
    private AvaloniaList<NitroxBlog> nitroxBlogs = [];

    public BlogViewModel()
    {
    }

    public BlogViewModel(NitroxBlogService nitroxBlogService)
    {
        this.nitroxBlogService = nitroxBlogService;
    }

    internal override async Task ViewContentLoadAsync(CancellationToken cancellationToken = default)
    {
        if (IsDesignMode)
        {
            return;
        }
        if (NitroxBlogs.Count <= 0)
        {
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                try
                {
                    NitroxBlogs.Clear();
                    await foreach (NitroxBlog? blog in nitroxBlogService?.GetBlogPostsAsync(cancellationToken)!)
                    {
                        NitroxBlogs.Add(blog);
                    }
                }
                catch (OperationCanceledException)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        LauncherNotifier.Error("Failed to fetch Nitrox blogs");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error while trying to display nitrox blogs");
                }
            });
        }
    }

    [RelayCommand]
    private void BlogEntryClick(string blogUrl)
    {
        OpenUri(blogUrl);
    }
}
