using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views;
using NitroxModel.Helper;

namespace Nitrox.Launcher.Models.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection collection)
    {
        // Avalonia and Reactive services
        collection.AddSingleton(provider => new AppViewLocator(provider));
        collection.AddSingleton<IRoutingScreen, RoutingScreen>();
        collection.AddSingleton<IDialogService>(provider => new DialogService(
                                                    new DialogManager(
                                                        provider.GetRequiredService<AppViewLocator>(),
                                                        new DialogFactory()),
                                                    provider.GetRequiredService));

        // Domain services
        collection.AddSingleton(_ => KeyValueStore.Instance);
        collection.AddSingleton<ServerService>();
        
        // Dialog ViewModels and Dialog Views
        collection.AddTransient<CreateServerViewModel>();
        collection.AddTransient<CreateServerModal>();
        collection.AddTransient<BackupRestoreViewModel>();
        collection.AddTransient<BackupRestoreModal>();
        collection.AddTransient<ObjectPropertyEditorViewModel>();
        collection.AddTransient<ObjectPropertyEditorModal>();
        collection.AddTransient<DialogBoxViewModel>();
        collection.AddTransient<DialogBoxModal>();

        // Views
        collection.AddSingleton<LaunchGameView>();
        collection.AddSingleton<OptionsView>();
        collection.AddSingleton<ServersView>();
        collection.AddSingleton<ManageServerView>();
        collection.AddSingleton<BlogView>();
        collection.AddSingleton<CommunityView>();
        collection.AddSingleton<UpdatesView>();
        collection.AddSingleton<EmbeddedServerView>();

        // ViewModels
        collection.AddTransient<MainWindowViewModel>();
        collection.AddTransient<LaunchGameViewModel>();
        collection.AddTransient<OptionsViewModel>();
        collection.AddTransient<ServersViewModel>();
        collection.AddTransient<ManageServerViewModel>();
        collection.AddTransient<BlogViewModel>();
        collection.AddTransient<CommunityViewModel>();
        collection.AddTransient<UpdatesViewModel>();
        collection.AddTransient<EmbeddedServerViewModel>();

        return collection;
    }
}
