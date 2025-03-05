using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Extensions;

public static class ScreenExtensions
{
    private static readonly List<RoutableViewModelBase> navigationStack = [];

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
        if (screen.ActiveViewModel is RoutableViewModelBase routableViewModelBase)
        {
            navigationStack.RemoveAllFast(screen.ActiveViewModel, (item, param) => item.GetType() == param.GetType());
            await routableViewModelBase.ViewContentUnloadAsync();
            navigationStack.Add(routableViewModelBase);
        }
        Stopwatch sw = Stopwatch.StartNew();
        Task contentLoadTask = routableViewModel.ViewContentLoadAsync();
        if (screen.ActiveViewModel != null)
        {
            // Only show loading screen if page isn't loading super quickly.
            await Task.Delay(50);
            if (!contentLoadTask.IsCompleted)
            {
                screen.ActiveViewModel = AssetHelper.GetFullAssetPath("/Assets/Icons/loading.svg");
                await Task.Delay((int)Math.Max(0, 500 - sw.Elapsed.TotalMilliseconds));
            }
        }
        await contentLoadTask;
        screen.ActiveViewModel = routableViewModel;
    }

    public static async Task<bool> BackAsync(this IRoutingScreen screen)
    {
        if (navigationStack.Count < 1)
        {
            return false;
        }
        RoutableViewModelBase backViewModel = navigationStack[^1];
        navigationStack.Remove(backViewModel);
        await ShowAsync(screen, backViewModel);
        return true;
    }

    /// <summary>
    ///     Tries to go back to the view assigned to the given ViewModel.
    /// </summary>
    /// <returns>
    ///     True if ViewModel was found in the routing navigation stack. False when the ViewModel wasn't found and routing
    ///     failed.
    /// </returns>
    public static async Task<bool> BackToAsync<T>(this IRoutingScreen screen) where T : RoutableViewModelBase
    {
        for (int i = navigationStack.Count - 1; i >= 0; i--)
        {
            if (navigationStack[i] is T target)
            {
                // Cleanup the stack up and including the back-target.
                for (int j = i; j < navigationStack.Count; j++)
                {
                    navigationStack.RemoveAt(j);
                }
                await screen.ShowAsync(target);
                return true;
            }
        }
        return false;
    }
}
