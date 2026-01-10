using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Nitrox.Launcher.Models.Attributes;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.HttpDelegatingHandlers;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views;
using Nitrox.Launcher.Views.Abstract;
using Nitrox.Model.Helper;
using ServiceScan.SourceGenerator;

namespace Nitrox.Launcher.Models.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services) =>
        services
            // Domain APIs
            .AddSingleton(_ => KeyValueStore.Instance)
            .AddHttp()
            .AddMagicOnionApis()
            // Services
            .AddMagicOnion().Services
            .AddHostedSingletonService<StopAvaloniaOnHostExitService>()
            .AddHostedSingletonService<PreventMultipleAppInstancesService>()
            .AddHostedSingletonService<WriteGrpcPortFileService>()
            .AddSingleton<ServerService>()
            .AddSingleton<DialogService>()
            .AddSingleton<StorageService>()
            .AddSingleton<BackupService>()
            // UI
            .AddSingleton<Window, MainWindow>()
            .AddSingleton<MainWindowViewModel>()
            .AddSingleton<Func<IRoutingScreen>>(provider => provider.GetRequiredService<MainWindowViewModel>)
            .AddSingleton<Func<Window>>(provider => () =>
            {
                Window window = provider.GetRequiredService<Window>();
                window.DataContext = provider.GetRequiredService<MainWindowViewModel>();
                return window;
            })
            .AddDialogs()
            .AddViews()
            .AddViewModels();

    private static IServiceCollection AddHostedSingletonService<T>(this IServiceCollection services) where T : class, IHostedService => services.AddSingleton<T>().AddHostedService(provider => provider.GetRequiredService<T>());

    [GenerateServiceRegistrations(AttributeFilter = typeof(ModalForViewModelAttribute), CustomHandler = nameof(AddDialog))]
    private static partial IServiceCollection AddDialogs(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(RoutableViewBase<>), ExcludeAssignableTo = typeof(MainWindow), AsSelf = true)]
    private static partial IServiceCollection AddViews(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), ExcludeAssignableTo = typeof(MainWindowViewModel), AsSelf = true, Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddViewModels(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(DelegatingHandler))]
    [GenerateServiceRegistrations(AssignableTo = typeof(DelegatingHandler), AsSelf = true)]
    private static partial IServiceCollection AddHttpClientDelegatingHandlers(this IServiceCollection services);

    [GenerateServiceRegistrations(AttributeFilter = typeof(HttpServiceAttribute), CustomHandler = nameof(InternalAddHttpClient))]
    private static partial IServiceCollection AddHttpClients(this IServiceCollection services);

    private static IServiceCollection AddMagicOnionApis(this IServiceCollection services) =>
        services.AddMagicOnion().Services;

    /// <remarks>
    ///     <a
    ///         href="https://learn.microsoft.com/en-us/dotnet/core/extensions/httpclient-factory#using-ihttpclientfactory-together-with-socketshttphandler">
    ///         This code is from MSDN.
    ///     </a>
    /// </remarks>
    private static void InternalAddHttpClient<T>(this IServiceCollection services) where T : class =>
        services.AddHttpClient<T>()
                .UseSocketsHttpHandler((handler, _) => handler.PooledConnectionLifetime =
                                           TimeSpan.FromMinutes(2)) // Recreate connection every 2 minutes (refreshes DNS)
                .SetHandlerLifetime(Timeout.InfiniteTimeSpan); // Disable rotation, as it is handled by PooledConnectionLifetime.

    private static void AddDialog<TDialog>(this IServiceCollection services) where TDialog : ModalBase
    {
        services.AddTransient<TDialog>();
        services.TryAddTransient(GetViewModelType());
        services.AddSingleton(provider => new DialogService.Mapping(GetViewModelType(), viewModel =>
        {
            TDialog dialog = provider.GetRequiredService<TDialog>();
            dialog.DataContext = provider.GetRequiredService(viewModel);
            return dialog;
        }));
        static Type GetViewModelType() => typeof(TDialog).GetCustomAttribute<ModalForViewModelAttribute>()?.ViewModelType ?? throw new Exception($"No ViewModel assigned to {typeof(TDialog).Name}");
    }

    private static IServiceCollection AddHttp(this IServiceCollection services) =>
        services.AddHttpClientDelegatingHandlers()
                .ConfigureHttpClientDefaults(builder =>
                {
                    builder.AddHttpMessageHandler<CacheGetRequestTaskDelegatingHandler>()
                           .AddHttpMessageHandler<FileCacheDelegatingHandler>()
                           .AddHttpMessageHandler<LogRequestDelegatingHandler>();
                    builder.ConfigureHttpClient(client =>
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("Nitrox.Launcher");
                        client.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue { MaxAge = TimeSpan.FromDays(1) };
                        client.Timeout = TimeSpan.FromSeconds(10);
                    });
                })
                .AddHttpClients();
}
