using System;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Threading;
using HanumanInstitute.MvvmDialogs;
using Nitrox.Launcher.Models.Design;
using Nitrox.Launcher.Models.Utils;
using Nitrox.Launcher.ViewModels;
using NitroxModel.Helper;
using NitroxModel.Logger;
using ReactiveUI;

namespace Nitrox.Launcher.Models.Services;

public class ServerService
{
    private readonly IDialogService dialogService;
    private readonly IKeyValueStore keyValueStore;
    private readonly IScreen screen;

    public ServerService(IDialogService dialogService, IKeyValueStore keyValueStore, IScreen screen)
    {
        this.dialogService = dialogService;
        this.keyValueStore = keyValueStore;
        this.screen = screen;
    }

    public async Task<bool> StartServerAsync(ServerEntry server)
    {
        // TODO: Exclude upgradeable versions + add separate prompt to upgrade first?
        if (server.Version != NitroxEnvironment.Version && !await ConfirmServerVersionAsync(server))
        {
            return false;
        }
        if (await GameInspect.IsOutdatedGameAndNotify(NitroxUser.GamePath, dialogService))
        {
            return false;
        }

        try
        {
            server.Version = NitroxEnvironment.Version;
            server.Start(keyValueStore.GetSavesFolderDir());
            if (server.IsEmbedded)
            {
                screen.Show(new EmbeddedServerViewModel(server));
            }
            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while starting server \"{server.Name}\"");
            await Dispatcher.UIThread.InvokeAsync(async () => await dialogService.ShowErrorAsync(ex, $"Error while starting server \"{server.Name}\""));
            return false;
        }
    }

    public async Task<bool> ConfirmServerVersionAsync(ServerEntry server) =>
        await dialogService.ShowAsync<DialogBoxViewModel>(model =>
        {
            model.Description = $"The version of '{server.Name}' is v{(server.Version != null ? server.Version.ToString() : "X.X.X.X")}. It is highly recommended to NOT use this save file with Nitrox v{NitroxEnvironment.Version}. Would you still like to continue?";
            model.DescriptionFontSize = 24;
            model.DescriptionFontWeight = FontWeight.Bold;
            model.ButtonOptions = ButtonOptions.YesNo;
        });
}
