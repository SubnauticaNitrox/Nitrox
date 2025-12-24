using System.Threading.Tasks;
using MagicOnion;

namespace Nitrox.Model.MagicOnion;

/// <summary>
///     See <a href="https://cysharp.github.io/MagicOnion/streaminghub/getting-started#steps">MagicOnion docs</a>
/// </summary>
public interface IServersManagement : IStreamingHub<IServersManagement, IServerManagementReceiver>
{
    ValueTask SetPlayerCount(int playerCount);
    ValueTask AddOutputLine(string category, string time, int level, string message);
}

/// <summary>
///     The client-side interface. However in this case, the launcher project is the gRPC server and the game server is the
///     gRPC client.
/// </summary>
public interface IServerManagementReceiver
{
    void OnCommand(string command);
}
