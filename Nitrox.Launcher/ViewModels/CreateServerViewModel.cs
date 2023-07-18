using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Server;

namespace Nitrox.Launcher.ViewModels;

public partial class CreateServerViewModel : ModalViewModelBase
{
    [ObservableProperty]
    private string name;
    [ObservableProperty]
    private ServerGameMode selectedGameMode = ServerGameMode.SURVIVAL;
    public KeyGesture BackHotkey { get; } = new(Key.Escape);
    public KeyGesture CreateHotkey { get; } = new(Key.Return);

    [RelayCommand(CanExecute = nameof(CanCreate))]
    private void Create(Window window)
    {
        DialogResult = true;
        Close(window);
    }

    private bool CanCreate()
    {
        // TODO: Validate that the name isn't a duplicate of another save
        return true;
    }
}
