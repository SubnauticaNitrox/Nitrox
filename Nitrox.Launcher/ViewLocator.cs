using System;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher;

public class AppViewLocator : IViewLocator
{
    public IViewFor ResolveView<T>(T viewModel, string? contract = null) => viewModel switch
    {
        PlayViewModel context => new PlayView { DataContext = context },
        EmptyServersViewModel context => new EmptyServersView { DataContext = context },
        _ => throw new ArgumentOutOfRangeException(nameof(viewModel))
    };
}
