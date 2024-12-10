using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Extensions;

public static class ScreenExtensions
{
    /// <summary>
    ///     Navigates to a view assigned to the given ViewModel.
    /// </summary>
    /// <param name="screen">The screen used to display the view.</param>
    /// <param name="routableViewModel">ViewModel that should be shown.</param>
    /// <typeparam name="TViewModel">Type of the ViewModel to show.</typeparam>
    public static void Show<TViewModel>(this IScreen screen, TViewModel routableViewModel) where TViewModel : RoutableViewModelBase
    {
        if (screen == null)
        {
            return;
        }
        // When navigating away from a view in an async button command, busy states on buttons should also reset. Otherwise, when navigating back it would still show buttons being busy.
        NitroxAttached.AsyncCommandButtonTagger.Clear();
        screen.Router.Navigate.Execute(routableViewModel);
    }

    public static void Back(this IScreen screen) => screen.Router.NavigateBack.Execute();

    public static bool Back<TViewModel>(this IScreen screen) where TViewModel : IRoutableViewModel
    {
        if (screen == null)
        {
            return false;
        }
        if (screen.Router.FindViewModelInStack<TViewModel>() is not RoutableViewModelBase routableViewModel)
        {
            return false;
        }
        screen.Show(routableViewModel);
        return true;
    }

    public static bool BackAny<T1, T2>(this IScreen screen) where T1 : IRoutableViewModel where T2 : IRoutableViewModel
    {
        if (screen.Back<T1>())
        {
            return true;
        }
        return screen.Back<T2>();
    }
}
