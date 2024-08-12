using System;
using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Converters;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class BlogViewModel : RoutableViewModelBase
{
    public Bitmap FallbackImage { get; set; } = BitmapAssetValueConverter.GetBitmapFromPath("/Assets/Images/blog/vines.png");

    [ObservableProperty]
    private AvaloniaList<NitroxBlog> nitroxBlogs = [];

    public BlogViewModel(IScreen screen, NitroxBlog[] blogs = null) : base(screen)
    {
        nitroxBlogs.AddRange(blogs ?? []);
        Dispatcher.UIThread.Invoke(async () =>
        {
            try
            {
                nitroxBlogs.Clear();
                nitroxBlogs.AddRange(await Downloader.GetBlogs());
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display nitrox blogs");
            }
        });
    }

    [RelayCommand]
    private void BlogEntryClick(string blogUrl)
    {
        UriBuilder blogUriBuilder = new(blogUrl)
        {
            Scheme = Uri.UriSchemeHttps,
            Port = -1
        };

        Process.Start(new ProcessStartInfo(blogUriBuilder.Uri.ToString()) { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}
