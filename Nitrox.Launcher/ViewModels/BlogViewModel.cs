using System;
using System.Threading.Tasks;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;

namespace Nitrox.Launcher.ViewModels;

public partial class BlogViewModel : RoutableViewModelBase
{
    public static Bitmap FallbackImage { get; } = AssetHelper.GetAssetFromStream("/Assets/Images/blog/vines.png", static stream => new Bitmap(stream));

    [ObservableProperty]
    private AvaloniaList<NitroxBlog> nitroxBlogs = [];

    public BlogViewModel()
    {
    }

    internal override async Task ViewContentLoadAsync()
    {
        if (Design.IsDesignMode)
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
                    NitroxBlogs.AddRange(await Downloader.GetBlogsAsync());
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
        ProcessUtils.OpenUrl(blogUrl);
    }
}
