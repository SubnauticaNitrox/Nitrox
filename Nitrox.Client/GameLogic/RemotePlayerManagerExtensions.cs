using Nitrox.Model.DataStructures;

namespace Nitrox.Client.GameLogic
{
    public static class RemotePlayerManagerExtensions
    {
        public static bool TryFind(this PlayerManager playerManager, ushort playerId, out RemotePlayer remotePlayer)
        {
            Optional<RemotePlayer> optional = playerManager.Find(playerId);
            remotePlayer = optional.HasValue ? optional.Value : null;

            return optional.HasValue;
        }
    }
}
