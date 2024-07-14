using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Extensions;

public static class ScreenExtensions
{
    public static void Show<TViewModel>(this IScreen screen, TViewModel routableViewModel) where TViewModel : RoutableViewModelBase
    {
        screen.Router.Navigate.Execute(routableViewModel);
    }

    public static void Back(this IScreen screen)
    {
        screen.Router.NavigateBack.Execute();
    }
}
