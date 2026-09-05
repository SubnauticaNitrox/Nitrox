using System;
using System.Linq;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class UnlockAchievementProcessor : IClientPacketProcessor<UnlockAchievement>
{
    public Task Process(ClientProcessorContext context, UnlockAchievement packet)
    {
        if (Enum.TryParse(packet.AchievementId, true, out GameAchievements.Id id) && id != GameAchievements.Id.None)
        {
            GameAchievements.Unlock(id);
            Log.InGame(Language.main.Get("Nitrox_AchievementUnlocked").Replace("{ACHIEVEMENT}", id.ToString()));
        }
        else
        {
            string validNames = string.Join(", ", Enum.GetNames(typeof(GameAchievements.Id)).Where(name => name != nameof(GameAchievements.Id.None)));
            Log.InGame(Language.main.Get("Nitrox_AchievementUnknown").Replace("{ACHIEVEMENT}", packet.AchievementId).Replace("{LIST}", validNames));
        }
        return Task.CompletedTask;
    }
}
