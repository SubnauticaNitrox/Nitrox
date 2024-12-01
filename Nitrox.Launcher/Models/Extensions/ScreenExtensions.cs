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
    public static void Show<TViewModel>(this IScreen screen, TViewModel routableViewModel) where TViewModel : RoutableViewModelBase => screen.Router.Navigate.Execute(routableViewModel);

    public static void Back(this IScreen screen) => screen.Router.NavigateBack.Execute();
}
