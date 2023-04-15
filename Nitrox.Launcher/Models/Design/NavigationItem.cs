using System.Windows.Input;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Design;

public class NavigationItem : INavigationItem
{
    private readonly string icon;

    public ICommand ClickCommand { get; init; }

    public string ToolTipText { get; init; }

    public string Text { get; }

    public string Icon
    {
        get => icon;
        init => icon = value ?? string.Empty;
    }

    public NavigationItem(string text)
    {
        Text = text;
        ClickCommand = ReactiveCommand.Create(() => { });
    }
}
