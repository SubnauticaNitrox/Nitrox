using NitroxModel.DataStructures.Util;

namespace NitroxClient.GameLogic
{
    public static class RemotePlayerManagerExtensions
    {
        public static bool TryFind(this PlayerManager playerManager, ushort playerId, out RemotePlayer remotePlayer)
        {
            Optional<RemotePlayer> optional = playerManager.Find(playerId);
            remotePlayer = optional.IsPresent() ? optional.Get() : null;

            return optional.IsPresent();
        }

        public static bool TryFindByName(this PlayerManager playerManager, string playerName, out RemotePlayer remotePlayer)
        {
            Optional<RemotePlayer> optional = playerManager.FindByName(playerName);
            remotePlayer = optional.IsPresent() ? optional.Get() : null;

            return optional.IsPresent();
        }
    }
}
