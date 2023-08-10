using System.Windows.Input;
using Avalonia.Media;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Design;

public class TitleBarItem : ReactiveObject
{
    private string icon = "";
    private ICommand command = ReactiveCommand.Create(() => { });
    private SolidColorBrush hoverBackgroundColor = new(Color.Parse("#333333"));

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

    public SolidColorBrush HoverBackgroundColor
    {
        get => hoverBackgroundColor;
        set => this.RaiseAndSetIfChanged(ref hoverBackgroundColor, value);
    }
}
