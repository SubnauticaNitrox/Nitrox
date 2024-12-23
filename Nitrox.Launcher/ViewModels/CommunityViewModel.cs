using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;

namespace Nitrox.Launcher.ViewModels;

public partial class CommunityViewModel : RoutableViewModelBase
{
    [RelayCommand]
    private void DiscordLink()
    {
        ProcessUtils.OpenUrl("discord.gg/E8B4X9s");
    }

    [RelayCommand]
    private void TwitterLink()
    {
        ProcessUtils.OpenUrl("twitter.com/modnitrox");
    }

    [RelayCommand]
    private void RedditLink()
    {
        ProcessUtils.OpenUrl("reddit.com/r/SubnauticaNitrox");
    }

    [RelayCommand]
    private void BlueskyLink()
    {
        ProcessUtils.OpenUrl("bsky.app/profile/nitroxmod.bsky.social");
    }

    [RelayCommand]
    private void GithubLink()
    {
        ProcessUtils.OpenUrl("github.com/SubnauticaNitrox/Nitrox");
    }
}
