using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;

namespace Nitrox.Launcher.ViewModels.Abstract;

/// <summary>
///     Base class for (popup) dialog ViewModels.
/// </summary>
public abstract partial class ModalViewModelBase : ObservableValidator, IModalDialogViewModel
{
    [ObservableProperty] private bool? dialogResult;

    [RelayCommand]
    public void Close(Window window)
    {
        window.Close(DialogResult);
    }

    public static implicit operator bool(ModalViewModelBase self)
    {
        return self is { DialogResult: true };
    }
}
