using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

internal partial class CommunityViewModel : RoutableViewModelBase
{
    [RelayCommand]
    private void DiscordLink()
    {
        OpenUrl("discord.gg/E8B4X9s");
    }

    [RelayCommand]
    private void TwitterLink()
    {
        OpenUrl("twitter.com/modnitrox");
    }

    [RelayCommand]
    private void RedditLink()
    {
        OpenUrl("reddit.com/r/SubnauticaNitrox");
    }

    [RelayCommand]
    private void BlueskyLink()
    {
        OpenUrl("bsky.app/profile/nitroxmod.bsky.social");
    }

    [RelayCommand]
    private void GithubLink()
    {
        OpenUrl("github.com/SubnauticaNitrox/Nitrox");
    }
}
