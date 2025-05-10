using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.ViewModels;

public partial class CommunityViewModel : RoutableViewModelBase
{
    [RelayCommand]
    private void DiscordLink()
    {
        ProcessEx.OpenUrl("discord.gg/E8B4X9s");
    }

    [RelayCommand]
    private void TwitterLink()
    {
        ProcessEx.OpenUrl("twitter.com/modnitrox");
    }

    [RelayCommand]
    private void RedditLink()
    {
        ProcessEx.OpenUrl("reddit.com/r/SubnauticaNitrox");
    }

    [RelayCommand]
    private void BlueskyLink()
    {
        ProcessEx.OpenUrl("bsky.app/profile/nitroxmod.bsky.social");
    }

    [RelayCommand]
    private void GithubLink()
    {
        ProcessEx.OpenUrl("github.com/SubnauticaNitrox/Nitrox");
    }
}
