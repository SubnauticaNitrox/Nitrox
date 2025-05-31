using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Helper;
using ServiceScan.SourceGenerator;

namespace Nitrox.Launcher.Models.Extensions;

public static partial class ServiceCollectionExtensions
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

        return collection
               .AddDialogs()
               .AddViews()
               .AddViewModels();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(ModalViewModelBase), AsSelf = true)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ModalBase), AsSelf = true)]
    private static partial IServiceCollection AddDialogs(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(RoutableViewBase<>), AsSelf = true, Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddViews(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), AsSelf = true)]
    private static partial IServiceCollection AddViewModels(this IServiceCollection services);
}
