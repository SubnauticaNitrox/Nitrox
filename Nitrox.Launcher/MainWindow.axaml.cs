using System;
using System.Collections.Generic;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;
using Serilog;

namespace Nitrox.Launcher;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    private readonly HashSet<Exception> handledExceptions = new();

    public MainWindow()
    {
        // Handle thrown exceptions so they aren't hidden.
        AppDomain.CurrentDomain.UnhandledException += (_, args) =>
        {
            if (args.ExceptionObject is Exception ex)
            {
                UnhandledExceptionHandler(ex);
            }
        };
        TaskScheduler.UnobservedTaskException += (_, args) =>
        {
            if (!args.Observed)
            {
                UnhandledExceptionHandler(args.Exception);
            }
        };

        RxApp.DefaultExceptionHandler = Observer.Create<Exception>(UnhandledExceptionHandler);

        this.WhenActivated(d =>
        {
            // Set clicked nav item as selected (and deselect the others).
            Button lastClickedNav = null;
            d(Button.ClickEvent.Raised.Subscribe(tuple =>
            {
                if (tuple.Item2.Source is Button btn && btn.Classes.Contains("nav"))
                {
                    lastClickedNav?.SetValue(NitroxAttached.SelectedProperty, false);
                    lastClickedNav = btn;
                    btn.SetValue(NitroxAttached.SelectedProperty, true);
                }
            }));

            try
            {
                ViewModel?.DefaultView.Execute(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to execute {nameof(ViewModel.DefaultView)} command");
            }
        });

        InitializeComponent();
    }

    private async void UnhandledExceptionHandler(Exception ex)
    {
        if (handledExceptions.Contains(ex))
        {
            return;
        }
        handledExceptions.Add(ex);

        await ViewModel?.ShowDialogAsync<ErrorViewModel>(vm => vm.Exception = ex)!;
    }

    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e) => BeginMoveDrag(e);
}
