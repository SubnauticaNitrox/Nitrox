using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Each (embedded) running server should have its own ViewModel.
/// </summary>
public partial class EmbeddedServerViewModel : RoutableViewModelBase
{
    [ObservableProperty]
    private string serverCommand;

    [ObservableProperty]
    private ServerEntry serverEntry;

    [ObservableProperty]
    private bool shouldAutoScroll = true;

    public AvaloniaList<string> ServerOutput => ServerEntry.Process.Output;

    public EmbeddedServerViewModel(IScreen screen, ServerEntry serverEntry) : base(screen)
    {
        this.serverEntry = serverEntry;
        this.WhenActivated(disposables =>
        {
            this.WhenAnyValue(model => model.ServerEntry.IsOnline)
                .Where(isOnline => !isOnline)
                .Subscribe(_ => screen.Back())
                .DisposeWith(disposables);
        });
    }

    [RelayCommand]
    private async Task SendServerCommandAsync()
    {
        if (ServerEntry.Process == null)
        {
            return;
        }
        await ServerEntry.Process.SendCommandAsync(ServerCommand);
        ServerCommand = "";
    }
    
    [RelayCommand]
    private void OutputSizeChanged(SizeChangedEventArgs args)
    {
        if (ShouldAutoScroll && args.HeightChanged && args.Source is Visual visual)
        {
            // Without dispatcher invoke it sometimes doesn't scroll all the way down (control measure logic bug?).
            Dispatcher.UIThread.InvokeAsync(() => visual.FindAncestorOfType<ScrollViewer>()?.ScrollToEnd());
        }
    }
}
