using MagicOnion;
using MagicOnion.Server;
using Nitrox.Launcher.Models.Design;
using Nitrox.Model.MagicOnion;

namespace Nitrox.Launcher.Models.Services;

internal sealed class ServerTrackingApiService(ServerService serverService) : ServiceBase<IServerTrackingService>, IServerTrackingService
{
    private readonly ServerService serverService = serverService;

    public async UnaryResult SetProcessId(int processId, string saveName)
    {
        ServerEntry entry = serverService.GetServerEntryByProcessIdOrSaveName(processId, saveName);
        if (entry == null)
        {
            return;
        }
        entry.ProcessId = processId;
        entry.IsOnline = true;
        entry.IsServerClosing = false;
        entry.IsEmbedded = true;
    }

    public async UnaryResult SetPlayerCount(int processId, int playerCount)
    {
        ServerEntry entry = serverService.GetServerEntryByProcessIdOrSaveName(processId, "");
        if (entry == null)
        {
            return;
        }
        entry.Players = playerCount;
    }

    public async UnaryResult AddOutputLine(int processId, string category, string time, int level, string message)
    {
        ServerEntry entry = serverService.GetServerEntryByProcessIdOrSaveName(processId, "");
        entry?.Output.Add(new OutputLine
        {
            LogText = message,
            Timestamp = time.ToString(),
            Type = (OutputLineType)level
        });
    }
}
