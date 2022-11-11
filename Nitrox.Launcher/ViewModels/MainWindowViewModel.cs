using System.Collections.Generic;
using Nitrox.Launcher.Models.Design;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public class MainWindowViewModel : ReactiveObject, IScreen
{
    public Interaction<CreateServerViewModel, CreateServerViewModel?> CreateServerDialog { get; } = new();
    public RoutingState Router { get; } = new();
    public List<INavigationItem> NavigationHeaderItems { get; }
    public List<INavigationItem> NavigationFooterItems { get; }

    public MainWindowViewModel()
    {
        NavigationHeaderItems = new List<INavigationItem>
        {
            new NavigationHeader("PLAY"),
            new NavigationItem("Play game")
            {
                ToolTipText = "Play the game",  
                Icon = "/Assets/Images/material-design-icons/play.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            },
            new NavigationItem("Servers")
            {
                ToolTipText = "Configure and start the server",
                Icon = "/Assets/Images/material-design-icons/server.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new ServersViewModel(this)))
            },
            new NavigationItem("Library")
            {
                ToolTipText = "Configure your setup",
                Icon = "/Assets/Images/material-design-icons/library.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            },
            new NavigationHeader("EXPLORE"),
            new NavigationItem("Community")
            {
                ToolTipText = "Join the Nitrox community",
                Icon = "/Assets/Images/material-design-icons/community.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            },
            new NavigationItem("Blog")
            {
               ToolTipText = "Read the latest from the Dev Blog",
               Icon = "/Assets/Images/material-design-icons/blog.png",
               ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            }
        };

        NavigationFooterItems = new List<INavigationItem>
        {
            new NavigationItem("Updates")
            {
                Icon = "/Assets/Images/material-design-icons/download.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            },
            new NavigationItem("Options")
            {
                Icon = "/Assets/Images/material-design-icons/options.png",
                ClickCommand = ReactiveCommand.CreateFromObservable(() => Router.Navigate.Execute(new PlayViewModel(this)))
            }
        };
    }
}
