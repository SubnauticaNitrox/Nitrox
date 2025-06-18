using System;
using System.Net.Http;
using System.Net.Http.Headers;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Microsoft.Extensions.DependencyInjection;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.HttpDelegatingHandlers;
using Nitrox.Launcher.Models.Services;
using Nitrox.Launcher.ViewModels.Abstract;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Helper;
using ServiceScan.SourceGenerator;

namespace Nitrox.Launcher.Models.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddAppServices(this IServiceCollection services)
    {
        // Add Avalonia (and related frameworks) services
        services.AddSingleton(provider => new AppViewLocator(provider))
                .AddSingleton<IRoutingScreen, RoutingScreen>()
                .AddSingleton<IDialogService>(provider => new DialogService(
                                                  new DialogManager(
                                                      provider.GetRequiredService<AppViewLocator>(),
                                                      new DialogFactory()),
                                                  provider.GetRequiredService))
                .AddHttpClients()
                // Domain APIs
                .AddSingleton(_ => KeyValueStore.Instance)
                // Services
                .AddSingleton<ServerService>()
                // UI
                .AddDialogs()
                .AddViews()
                .AddViewModels();

        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(ModalViewModelBase), AsSelf = true)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ModalBase), AsSelf = true)]
    private static partial IServiceCollection AddDialogs(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(RoutableViewBase<>), AsSelf = true, Lifetime = ServiceLifetime.Singleton)]
    private static partial IServiceCollection AddViews(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), AsSelf = true)]
    private static partial IServiceCollection AddViewModels(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(DelegatingHandler))]
    [GenerateServiceRegistrations(AssignableTo = typeof(DelegatingHandler), AsSelf = true)]
    private static partial IServiceCollection AddHttpClientDelegatingHandlers(this IServiceCollection services);

    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClientDelegatingHandlers();
        services.ConfigureHttpClientDefaults(builder =>
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
        });
        services.AddHttpClient<NitroxWebsiteApiService>();
        services.AddHttpClient<NitroxBlogService>();
        services.AddHttpClient<HttpImageService>();
        return services;
    }
}
