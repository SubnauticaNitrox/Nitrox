namespace Nitrox.Launcher.Models.Design;

public class NavigationHeader : INavigationItem
{
    public string Text { get; }

    public NavigationHeader(string text)
    {
        Text = text;
    }
}
