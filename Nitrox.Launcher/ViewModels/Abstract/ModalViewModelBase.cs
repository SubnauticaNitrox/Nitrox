using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models;

namespace Nitrox.Launcher.ViewModels.Abstract;

/// <summary>
///     Base class for (popup) dialog ViewModels.
/// </summary>
public abstract partial class ModalViewModelBase : ObservableValidator, IModalDialogViewModel, IMessageReceiver
{
    [ObservableProperty] private ButtonOptions? selectedOption;

    bool? IModalDialogViewModel.DialogResult => (bool)this;

    protected ModalViewModelBase()
    {
        // Always run validation first so HasErrors is set (i.e. trigger CanExecute logic).
        ValidateAllProperties();
    }

    public static implicit operator bool(ModalViewModelBase self)
    {
        return self is { HasErrors: false } and not { SelectedOption: null or ButtonOptions.No };
    }

    /// <summary>
    ///     Closes the dialog window. By default, sets the dialog result as cancelled.
    /// </summary>
    /// <param name="buttonOptions">The dialog result to set before closing.</param>
    [RelayCommand]
    public void Close(ButtonOptions? buttonOptions = null)
    {
        if (buttonOptions != null)
        {
            SelectedOption = buttonOptions;
        }
        ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.Windows.FirstOrDefault(w => w.DataContext == this)?.CloseByUser();
    }

    public virtual void Dispose()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
    }
}
