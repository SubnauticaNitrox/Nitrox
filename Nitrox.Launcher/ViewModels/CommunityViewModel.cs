using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

internal partial class CommunityViewModel : RoutableViewModelBase
{
    [RelayCommand]
    private void DiscordLink()
    {
        OpenUri("discord.gg/E8B4X9s");
    }

    [RelayCommand]
    private void XLink()
    {
        OpenUri("x.com/modnitrox");
    }

    [RelayCommand]
    private void RedditLink()
    {
        OpenUri("reddit.com/r/SubnauticaNitrox");
    }

    [RelayCommand]
    private void BlueskyLink()
    {
        OpenUri("bsky.app/profile/nitroxmod.bsky.social");
    }

    [RelayCommand]
    private void GithubLink()
    {
        OpenUri("github.com/SubnauticaNitrox/Nitrox");
    }
}
