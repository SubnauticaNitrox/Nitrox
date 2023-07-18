using System.ComponentModel;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels.Abstract;

/// <summary>
///     Base class for (popup) dialog ViewModels.
/// </summary>
public abstract partial class ModalViewModelBase : ObservableObject, IModalDialogViewModel, IReactiveObject
{
    [ObservableProperty] private bool? dialogResult;

    [RelayCommand]
    public void Close(Window window)
    {
        window.Close(DialogResult);
    }

    public void RaisePropertyChanging(PropertyChangingEventArgs args) => OnPropertyChanging(args);

    public void RaisePropertyChanged(PropertyChangedEventArgs args) => OnPropertyChanged(args);
}
