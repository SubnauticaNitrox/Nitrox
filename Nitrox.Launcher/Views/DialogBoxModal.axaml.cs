using System.Threading.Tasks;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class DialogBoxModal : ModalBase
{
    public DialogBoxModal()
    {
        InitializeComponent();
    }

    private Task copyTask;

    private void CopyButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (!copyTask?.IsCompleted ?? false)
        {
            return;
        }
        CopyButton.Content = "Copied!";
        copyTask = Task.Delay(3000).ContinueWith(_ => Dispatcher.UIThread.InvokeAsync(() => CopyButton.Content = "Copy to clipboard"));
    }
}
