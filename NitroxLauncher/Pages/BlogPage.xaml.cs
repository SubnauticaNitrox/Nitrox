using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages;

public partial class BlogPage : PageBase
{
    public static readonly Uri BLOGS_LINK = new("https://nitroxblog.rux.gg/");

    [ObservableProperty]
    private bool isLoading = true;

    [ObservableProperty]
    private ObservableCollection<NitroxBlog> nitroxBlogs = new();

    public BlogPage()
    {
        InitializeComponent();

        Dispatcher.InvokeAsync(async() =>
        {
            CancellationTokenSource cancellationTokenSource = new(8000);

            try
            {
                IList<NitroxBlog> blogs = await Downloader.GetBlogsAsync(cancellationTokenSource.Token);
                NitroxBlogs = new(blogs);
                IsLoading = false;
            }
            catch (OperationCanceledException)
            {
                // Ignore
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display nitrox blogs");
            }
        });
    }
}
