using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using NitroxModel.Platforms.OS.Windows;
using ReactiveUI;
using Serilog;

namespace Nitrox.Launcher;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    private readonly IDialogService dialogService;
    private readonly HashSet<Exception> handledExceptions = [];

    public MainWindow(IDialogService dialogService)
    {
        this.dialogService = dialogService;
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
            Button lastClickedNav = OpenLaunchGameViewButton;
            d(Button.ClickEvent.Raised.Subscribe(tuple =>
            {
                if (tuple.Item2.Source is Button btn && btn.Parent?.Classes.Contains("nav") == true)
                {
                    lastClickedNav?.SetValue(NitroxAttached.SelectedProperty, false);
                    lastClickedNav = btn;
                    btn.SetValue(NitroxAttached.SelectedProperty, true);
                }
            }));

            try
            {
                ViewModel?.DefaultViewCommand.Execute(null);
            }
            catch (Exception ex)
            {
                Log.Error(ex, $"Failed to execute {nameof(ViewModel.DefaultViewCommand)} command");
            }
        });

        InitializeComponent();
        
        // Restore default window animations for Windows OS
        if (!Design.IsDesignMode)
        {
            IntPtr? windowHandle = GetTopLevel(this)?.TryGetPlatformHandle()?.Handle;
            if (windowHandle.HasValue)
            {
                WindowsApi.EnableDefaultWindowAnimations(windowHandle.Value);
            }
        }
    }

    private async void UnhandledExceptionHandler(Exception ex)
    {
        if (!handledExceptions.Add(ex))
        {
            return;
        }
        if (Design.IsDesignMode)
        {
            Debug.WriteLine(ex);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            await dialogService?.ShowAsync<ErrorViewModel>(vm => vm.Exception = ex)!;
        });
    }

    private void TitleBar_OnPointerPressed(object sender, PointerPressedEventArgs e) => BeginMoveDrag(e);

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e) => Focus(); // Allow for de-focusing textboxes when clicking outside of them.
}
