using System.Threading.Tasks;
using MagicOnion;

namespace Nitrox.Model.MagicOnion;

/// <summary>
///     See MagicOnion docs: <a href="https://cysharp.github.io/MagicOnion/streaminghub/getting-started#steps" />
/// </summary>
public interface IServersManagement : IStreamingHub<IServersManagement, IServerManagementReceiver>
{
    ValueTask SetPlayerCount(int processId, int playerCount);
    ValueTask AddOutputLine(int processId, string category, string time, int level, string message);
}

/// <summary>
///     The client-side interface. However in this case, the launcher project is the gRPC server and the game server is the
///     gRPC client.
/// </summary>
public interface IServerManagementReceiver
{
    Task<string> OnRequestSaveName();
    Task<int> OnRequestProcessId();
    void OnCommand(string command);
}
