using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

public class ViewModelBase : ReactiveObject
{
    protected Window MainWindow
    {
        get
        {
            if (Application.Current?.ApplicationLifetime is not ClassicDesktopStyleApplicationLifetime desktop)
            {
                throw new NotSupportedException("This Avalonia application is only supported on desktop environments.");
            }
            return desktop.MainWindow;
        }
    }

    /// <summary>
    ///     Shows the dialog (interaction) and returns the result when it closes.
    /// </summary>
    protected async Task<TModalViewModel?> ShowDialogAsync<TModalViewModel>(Interaction<TModalViewModel, TModalViewModel?> interaction) where TModalViewModel : ModalViewModelBase, new()
    {
        return await interaction.Handle(new());
    }
}
