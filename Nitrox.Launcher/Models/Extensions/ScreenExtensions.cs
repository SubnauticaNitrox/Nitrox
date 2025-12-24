using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.Models.Extensions;

internal static class ScreenExtensions
{
    private static readonly List<RoutableViewModelBase> navigationStack = [];
    private static CancellationTokenSource? viewChangeBusyCts;
    private static readonly Lock viewChangeBusyCtsLocker = new();

    /// <summary>
    ///     Navigates to a view assigned to the given ViewModel.
    /// </summary>
    /// <param name="screen">The screen used to display the view.</param>
    /// <param name="routableViewModel">ViewModel that should be shown.</param>
    /// <typeparam name="TViewModel">Type of the ViewModel to show.</typeparam>
    public static async Task ShowAsync<TViewModel>(this IRoutingScreen? screen, TViewModel routableViewModel) where TViewModel : RoutableViewModelBase
    {
        if (screen == null)
        {
            return;
        }
        CancellationToken ctsToken;
        lock (viewChangeBusyCtsLocker)
        {
            if (viewChangeBusyCts != null)
            {
                viewChangeBusyCts.Cancel();
                viewChangeBusyCts.Dispose();
            }
            viewChangeBusyCts = new CancellationTokenSource();
            ctsToken = viewChangeBusyCts.Token;
        }
        // When navigating away from a view in an async button command, busy states on buttons should also reset. Otherwise, when navigating back it would still show buttons being busy.
        NitroxAttached.AsyncCommandButtonTagger.Clear();
        if (screen.ActiveViewModel is RoutableViewModelBase priorViewModel)
        {
            navigationStack.RemoveAllFast(screen.ActiveViewModel, (item, param) => item.GetType() == param.GetType());
            await priorViewModel.ViewContentUnloadAsync();
            navigationStack.Add(priorViewModel);
        }
        else
        {
            priorViewModel = null;
        }

        try
        {
            ctsToken.ThrowIfCancellationRequested();
            Stopwatch sw = Stopwatch.StartNew();
            Task contentLoadTask = routableViewModel.ViewContentLoadAsync(ctsToken);
            if (screen.ActiveViewModel != null)
            {
                // Only show loading screen if page isn't loading super quickly.
                await Task.Delay(50, ctsToken);
                if (!contentLoadTask.IsCompleted)
                {
                    ctsToken.ThrowIfCancellationRequested();
                    screen.ActiveViewModel = AssetHelper.GetFullAssetPath("/Assets/Icons/loading.svg");
                    await Task.Delay((int)Math.Max(0, 500 - sw.Elapsed.TotalMilliseconds), ctsToken);
                }
            }
            await contentLoadTask;
            ctsToken.ThrowIfCancellationRequested();
            screen.ActiveViewModel = routableViewModel;
        }
        catch (OperationCanceledException)
        {
            if (priorViewModel != null && navigationStack.Count > 0)
            {
                navigationStack.Remove(navigationStack[^1]);
            }
        }
    }

    public static async Task<bool> BackAsync(this IRoutingScreen screen)
    {
        RoutableViewModelBase? backViewModel = null;
        while (navigationStack.Count > 0 && (backViewModel == null || backViewModel == screen.ActiveViewModel))
        {
            backViewModel = navigationStack[^1];
            navigationStack.Remove(backViewModel);
        }
        if (backViewModel != null)
        {
            await ShowAsync(screen, backViewModel);
        }
        return true;
    }

    /// <summary>
    ///     Tries to go back to the view assigned to the given ViewModel.
    /// </summary>
    /// <returns>
    ///     True if ViewModel was found in the routing navigation stack. False when the ViewModel wasn't found and routing
    ///     failed.
    /// </returns>
    public static async Task<bool> BackToAsync(this IRoutingScreen screen, Type? type)
    {
        if (type == null)
        {
            return await BackAsync(screen);
        }

        for (int i = navigationStack.Count - 1; i >= 0; i--)
        {
            RoutableViewModelBase target = navigationStack[i];
            if (type.IsAssignableFrom(target.GetType()))
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
