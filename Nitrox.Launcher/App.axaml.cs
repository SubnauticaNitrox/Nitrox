using Avalonia;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using Nitrox.Launcher.ViewModels;
using ReactiveUI;
using Splat;

namespace Nitrox.Launcher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        IMutableDependencyResolver build = Locator.CurrentMutable;
        build.RegisterLazySingleton(() => (IDialogService)new DialogService(
                                        new DialogManager(
                                            viewLocator: AppViewLocator.Instance.Value,
                                            dialogFactory: new DialogFactory()),
                                        viewModelFactory: x => Locator.Current.GetService(x)));

        SplatRegistrations.Register<MainWindowViewModel>();
        SplatRegistrations.Register<CreateServerViewModel>();
        SplatRegistrations.Register<ErrorViewModel>();
        SplatRegistrations.SetupIOC();
    }

    public override void OnFrameworkInitializationCompleted()
    {
        DialogService.Show(null, MainWindow);

        base.OnFrameworkInitializationCompleted();
    }

    public static MainWindowViewModel MainWindow => Locator.Current.GetService<MainWindowViewModel>()!;
    public static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;
}
