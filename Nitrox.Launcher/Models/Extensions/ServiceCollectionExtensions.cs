using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection collection)
    {
        // Domain services
        collection.AddSingleton(provider => new AppViewLocator(provider));

        // Avalonia and Reactive services
        collection.AddSingleton<IScreen, RoutingScreen>();
        collection.AddSingleton<IDialogService>(provider => new DialogService(
                                                    new DialogManager(
                                                        provider.GetRequiredService<AppViewLocator>(),
                                                        new DialogFactory()),
                                                    provider.GetRequiredService));

        // Dialog ViewModels and Dialog Views
        collection.AddSingleton<CreateServerViewModel>();
        collection.AddTransient<CreateServerModal>();
        collection.AddSingleton<ConfirmationBoxViewModel>();
        collection.AddTransient<ConfirmationBoxModal>();
        collection.AddSingleton<ErrorViewModel>();
        collection.AddTransient<ErrorModal>();

        // Views
        collection.AddSingleton(provider => new MainWindow(provider.GetRequiredService<IDialogService>()) { DataContext = provider.GetRequiredService<MainWindowViewModel>() });
        collection.AddSingleton<LaunchGameView>();
        collection.AddSingleton<OptionsView>();
        collection.AddSingleton<ServersView>();
        collection.AddSingleton<ManageServerView>();
        collection.AddSingleton<BlogView>();
        collection.AddSingleton<CommunityView>();
        collection.AddSingleton<UpdatesView>();

        // ViewModels
        collection.AddSingleton<MainWindowViewModel>();
        collection.AddSingleton<LaunchGameViewModel>();
        collection.AddSingleton<OptionsViewModel>();
        collection.AddSingleton<ServersViewModel>();
        collection.AddSingleton<ManageServerViewModel>();
        collection.AddSingleton<BlogViewModel>();
        collection.AddSingleton<CommunityViewModel>();
        collection.AddSingleton<UpdatesViewModel>();

        return collection;
    }
}
