using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia.Collections;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class BlogViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    private AvaloniaList<NitroxBlog> nitroxBlogs = [];
    
    public BlogViewModel(IScreen hostScreen) : base(hostScreen)
    {
        Dispatcher.UIThread.Invoke(new Action(async () =>
        {
            try
            {
                IList<NitroxBlog> blogs = await Downloader.GetBlogs();

                foreach (NitroxBlog blog in blogs)
                {
                    nitroxBlogs.Add(blog);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while trying to display nitrox blogs");
            }
        }));
    }

    [RelayCommand]
    private void BlogEntryClick(string blogUrl)
    {
        Process.Start(new ProcessStartInfo(blogUrl) { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}