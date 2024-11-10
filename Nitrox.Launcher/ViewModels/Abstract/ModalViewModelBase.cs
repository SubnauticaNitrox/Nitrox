using System;
using System.Linq;
using System.Reactive.Disposables;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;

namespace Nitrox.Launcher.ViewModels.Abstract;

/// <summary>
///     Base class for (popup) dialog ViewModels.
/// </summary>
public abstract partial class ModalViewModelBase : ObservableValidator, IModalDialogViewModel, IDisposable
{
    protected readonly CompositeDisposable Disposables = new();
    [ObservableProperty] private ButtonOptions? selectedOption;

    bool? IModalDialogViewModel.DialogResult => (bool)this;

    protected ModalViewModelBase()
    {
        // Always run validation first so HasErrors is set (i.e. trigger CanExecute logic).
        ValidateAllProperties();
    }

    public static implicit operator bool(ModalViewModelBase self) => self is { HasErrors: false } and not { SelectedOption: null or ButtonOptions.No };

    [RelayCommand]
    public void Close(ButtonOptions? buttonOptions = null)
    {
        if (buttonOptions != null)
        {
            SelectedOption = buttonOptions;
        }
        ((IClassicDesktopStyleApplicationLifetime)Application.Current?.ApplicationLifetime)?.Windows.FirstOrDefault(w => w.DataContext == this)?.Close();
    }

    public void Dispose() => Disposables.Dispose();

    [RelayCommand]
    public void Drag(PointerPressedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);

        if (args.Source is Visual element && element.GetWindow() is {} window)
        {
            window.BeginMoveDrag(args);
        }
    }
}
