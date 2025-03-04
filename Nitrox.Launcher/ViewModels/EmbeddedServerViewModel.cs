using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.DataStructures;

namespace Nitrox.Launcher.ViewModels;

/// <summary>
///     Each (embedded) running server should have its own ViewModel.
/// </summary>
public partial class EmbeddedServerViewModel : RoutableViewModelBase
{
    private readonly CircularBuffer<string> commandHistory = new(1000);
    private int? selectedHistoryIndex;

    [ObservableProperty]
    private string serverCommand;

    [ObservableProperty]
    private ServerEntry serverEntry;

    [ObservableProperty]
    private bool shouldAutoScroll = true;

    public AvaloniaList<OutputLine> ServerOutput => ServerEntry.Process.Output;

    public EmbeddedServerViewModel()
    {
    }

    public EmbeddedServerViewModel(ServerEntry serverEntry)
    {
        this.serverEntry = serverEntry;
        this.RegisterMessageListener<ServerStatusMessage, EmbeddedServerViewModel>(static (status, model) =>
        {
            if (status.Server != model.ServerEntry)
            {
                return;
            }
            if (!status.IsOnline && model.HostScreen.ActiveViewModel is EmbeddedServerViewModel)
            {
                model.HostScreen.BackAsync().ConfigureAwait(false);
            }
        });
    }
    
    [RelayCommand]
    private async Task BackAsync() => await HostScreen.BackToAsync<ServersViewModel>();

    [RelayCommand]
    private async Task SendServerAsync(TextBox textBox)
    {
        if (ServerEntry.Process == null)
        {
            return;
        }
        if (string.IsNullOrWhiteSpace(ServerCommand))
        {
            return;
        }
        if (commandHistory.Count < 1 || commandHistory[commandHistory.LastChangedIndex] != ServerCommand)
        {
            commandHistory.Add(ServerCommand);
            ServerOutput.Add(new OutputLine
            {
                Type = OutputLineType.COMMAND,
                Timestamp = $@"[{TimeSpan.FromTicks(DateTime.Now.Ticks):hh\:mm\:ss\.fff}]",
                LogText = $"> {ServerCommand}"
            });
        }
        await ServerEntry.Process.SendCommandAsync(ServerCommand);
        ClearInput(textBox);
    }
    
    [RelayCommand]
    private async Task StopServerAsync()
    {
        if (ServerEntry.Process == null)
        {
            return;
        }
        await ServerEntry.StopAsync();
    }

    [RelayCommand]
    private void ClearInput(TextBox textBox)
    {
        ServerCommand = "";
        selectedHistoryIndex = null;
        SetCaretToEnd(textBox);
    }

    [RelayCommand]
    private void CommandHistoryGoBack(TextBox textBox)
    {
        if (commandHistory.Count < 1)
        {
            return;
        }
        selectedHistoryIndex ??= 0;
        selectedHistoryIndex--;
        ServerCommand = commandHistory[selectedHistoryIndex.Value];
        SetCaretToEnd(textBox);
    }

    [RelayCommand]
    private void CommandHistoryGoForward(TextBox textBox)
    {
        if (commandHistory.Count < 1)
        {
            return;
        }
        selectedHistoryIndex ??= -1;
        selectedHistoryIndex++;
        ServerCommand = commandHistory[selectedHistoryIndex.Value];
        SetCaretToEnd(textBox);
    }

    [RelayCommand]
    private void OutputSizeChanged(SizeChangedEventArgs args)
    {
        if (ShouldAutoScroll && args.HeightChanged && args.Source is Visual visual)
        {
            ScrollViewer scrollViewer = visual.FindAncestorOfType<ScrollViewer>();
            if (scrollViewer is not null)
            {
                // TODO: ScrollToEnd for virtualized lists is not working well, see: https://github.com/AvaloniaUI/Avalonia/issues/14365 - wait for fix to clean up this code.
                // Workaround: Run ScrollToEnd twice on the next two frames.
                Dispatcher.UIThread.InvokeAsync(() =>
                {
                    scrollViewer.ScrollToEnd();
                    // Run it again next frame
                    Dispatcher.UIThread.InvokeAsync(() => scrollViewer.ScrollToEnd());
                });
            }
        }
    }

    private void SetCaretToEnd(TextBox textBox)
    {
        if (textBox is not { Text: { } text })
        {
            return;
        }
        textBox.CaretIndex = text.Length;
    }
}
