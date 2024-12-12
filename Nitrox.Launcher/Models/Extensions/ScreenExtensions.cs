using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Extensions;

public static class ScreenExtensions
{
    private static RoutableViewModelBase previous;

    /// <summary>
    ///     Navigates to a view assigned to the given ViewModel.
    /// </summary>
    /// <param name="screen">The screen used to display the view.</param>
    /// <param name="routableViewModel">ViewModel that should be shown.</param>
    /// <typeparam name="TViewModel">Type of the ViewModel to show.</typeparam>
    public static async Task ShowAsync<TViewModel>(this IRoutingScreen screen, TViewModel routableViewModel) where TViewModel : RoutableViewModelBase
    {
        if (screen == null)
        {
            return;
        }
        // When navigating away from a view in an async button command, busy states on buttons should also reset. Otherwise, when navigating back it would still show buttons being busy.
        NitroxAttached.AsyncCommandButtonTagger.Clear();
        if (screen.ActiveViewModel is not LoadingViewModel)
        {
            previous = screen.ActiveViewModel;
        }
        Stopwatch sw = Stopwatch.StartNew();
        Task contentLoadTask = routableViewModel.ViewContentLoadAsync();
        // Only show loading screen if page isn't loading super quickly.
        await Task.Delay(50);
        if (!contentLoadTask.IsCompleted)
        {
            screen.ActiveViewModel = new LoadingViewModel();
            await Task.Delay((int)Math.Max(0, 500 - sw.Elapsed.TotalMilliseconds));
        }
        await contentLoadTask;
        screen.ActiveViewModel = routableViewModel;
    }

    public static async Task BackAsync(this IRoutingScreen screen) => await ShowAsync(screen, previous);
}
