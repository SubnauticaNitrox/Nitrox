using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.Views;

public partial class MainWindow : WindowBase<MainWindowViewModel>
{
    private readonly IDialogService dialogService;
    private readonly HashSet<Exception> handledExceptions = [];

    // For designer
    public MainWindow()
    {
        InitializeComponent();
    }

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
            // On Linux systems, Avalonia has trouble allowing windows to resize without "decorations". So we enable it in full, but hide the custom titlebar as it'll look bad. 
            if (OperatingSystem.IsLinux())
            {
                SystemDecorations = SystemDecorations.Full;
                NitroxAttached.SetUseCustomTitleBar(this, false);
            }

            d(PointerPressedEvent.Raised.Subscribe(args =>
            {
                if (args.Item2 is { Handled: false, Source: Control { Tag: string url } control } && control.Classes.Contains("link"))
                {
                    Task.Run(() =>
                    {
                        UriBuilder urlBuilder = new(url)
                        {
                            Scheme = Uri.UriSchemeHttps,
                            Port = -1
                        };
                        Process.Start(new ProcessStartInfo(urlBuilder.Uri.ToString()) { UseShellExecute = true, Verb = "open" })?.Dispose();
                    });
                    args.Item2.Handled = true;
                }
            }));
        });

        InitializeComponent();
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

        string title = ex switch
        {
            { InnerException: { } inner } => inner.Message,
            _ => ex.Message
        };
        await dialogService.ShowErrorAsync(ex, $"Error: {title}");

        Environment.Exit(1);
    }
}
