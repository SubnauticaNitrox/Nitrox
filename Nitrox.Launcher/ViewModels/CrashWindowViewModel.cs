using System;
using System.Threading.Tasks;
using System.Web;
using Avalonia.Controls;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels.Abstract;
using NitroxModel.Discovery.Models;
using NitroxModel.Helper;

namespace Nitrox.Launcher.ViewModels;

public partial class CrashWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title;
    [ObservableProperty]
    private string message;

    [RelayCommand(CanExecute = nameof(CanRestart))]
    private void Restart()
    {
        ProcessUtils.StartSelf();
        Environment.Exit(0);
    }

    [RelayCommand]
    private void Report()
    {
        string errorTitle = Message[..Math.Min(Message.Length, 100)];
        try
        {
            errorTitle = Message.Substring(0, Math.Max(0, Math.Min(Message.IndexOf("at ", StringComparison.OrdinalIgnoreCase), Message.IndexOf('\n'))));
        }
        catch
        {
            // ignored
        }
        // TODO: Fill in more issue details (is latest release or commit, last view, last clicked button, etc).
        string issueTitle = $"Launcher v{NitroxEnvironment.Version} crashed with {errorTitle}";
        string whatHappened = $"```\n{Message}\n```";
        string storeType = NitroxUser.GamePlatform.Platform switch
        {
            Platform.STEAM => "Steam",
            Platform.EPIC => "Epic",
            Platform.MICROSOFT => "MS-Store",
            _ => "Other"
        };
        string createGithubIssueUrl = $"https://github.com/SubnauticaNitrox/Nitrox/issues/new?assignees=&labels=Type%3A+bug%2CStatus%3A+to+verify&projects=&template=bug_report.yaml&title={HttpUtility.UrlEncode(issueTitle)}&what_happened={HttpUtility.UrlEncode(whatHappened)}&os_type={HttpUtility.UrlEncode(GetOsType())}&store_type={HttpUtility.UrlEncode(storeType)}";
        ProcessUtils.OpenUrl(createGithubIssueUrl);

        static string GetOsType()
        {
            if (OperatingSystem.IsWindows())
            {
                return "Windows";
            }
            if (OperatingSystem.IsMacOS())
            {
                return "MacOS";
            }
            if (OperatingSystem.IsLinux())
            {
                return "Linux";
            }
            return "Windows"; // No "Other" option in issue template so "Windows" is default.
        }
    }

    [RelayCommand(AllowConcurrentExecutions = false)]
    private async Task CopyToClipboard(ContentControl commandControl)
    {
        IClipboard clipboard = commandControl?.GetWindow().Clipboard;
        if (clipboard != null)
        {
            await clipboard.SetTextAsync(Message);

            object previousContent = commandControl.Content;
            commandControl.Content = "Copied!";
            await Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await Task.Delay(3000);
                commandControl.Content = previousContent;
            });
        }
    }

    private bool CanRestart() => !string.IsNullOrWhiteSpace(NitroxUser.ExecutableFilePath ?? Environment.ProcessPath);
}
