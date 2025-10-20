using System;
using MagicOnion;

namespace Nitrox.Model.MagicOnion;

public interface IServerTrackingService : IService<IServerTrackingService>
{
    UnaryResult SetProcessId(int processId, string saveName);
    UnaryResult SetPlayerCount(int processId, int playerCount);
    UnaryResult AddOutputLine(int processId, string category, string time, int level, string message);
}
