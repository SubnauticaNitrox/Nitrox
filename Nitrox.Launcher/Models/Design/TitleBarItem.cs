using System.Windows.Input;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Design;

public class TitleBarItem : ReactiveObject
{
    private string icon;
    private ICommand command;

    public string Icon
    {
        get => icon;
        set => this.RaiseAndSetIfChanged(ref icon, value);
    }

    public ICommand Command
    {
        get => command;
        set => this.RaiseAndSetIfChanged(ref command, value);
    }

    public TitleBarItem()
    {
        icon = "";
        Command = ReactiveCommand.Create(() => { });
    }
}
