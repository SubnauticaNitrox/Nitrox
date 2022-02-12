using System;
using NitroxLauncher.Models;

namespace NitroxLauncher.Pages
{
    public partial class CommunityPage : PageBase
    {
        public static readonly Uri DISCORD_LINK = new("https://discord.gg/E8B4X9s");
        public static readonly Uri TWITTER_LINK = new("https://twitter.com/modnitrox");
        public static readonly Uri REDDIT_LINK = new("https://reddit.com/r/SubnauticaNitrox");
        public static readonly Uri GITHUB_LINK = new("https://github.com/SubnauticaNitrox/Nitrox");

        public CommunityPage()
        {
            InitializeComponent();
        }
    }
}
