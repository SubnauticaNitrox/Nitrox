﻿using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.ViewModels.Abstract;
using ReactiveUI;

namespace Nitrox.Launcher.ViewModels;

public partial class CommunityViewModel : RoutableViewModelBase
{
    public CommunityViewModel(IScreen screen) : base(screen)
    {
    }
    
    [RelayCommand]
    private void DiscordLink()
    {
        Process.Start(new ProcessStartInfo("https://discord.gg/E8B4X9s") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
    
    [RelayCommand]
    private void TwitterLink()
    {
        Process.Start(new ProcessStartInfo("https://twitter.com/modnitrox") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
    
    [RelayCommand]
    private void RedditLink()
    {
        Process.Start(new ProcessStartInfo("https://reddit.com/r/SubnauticaNitrox") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
    
    [RelayCommand]
    private void GithubLink()
    {
        Process.Start(new ProcessStartInfo("https://github.com/SubnauticaNitrox/Nitrox") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}