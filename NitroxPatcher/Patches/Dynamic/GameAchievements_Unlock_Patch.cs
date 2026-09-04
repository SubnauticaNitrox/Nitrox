using System.Reflection;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Settings;
using Nitrox.Model.DataStructures.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Straddles the achievement unlock gate to allow us to tie in the server's <see cref="AchievementsMode"/>
/// </summary>
public sealed partial class GameAchievements_Unlock_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => GameAchievements.Unlock(default));

    public static bool Prefix(GameAchievements.Id id)
    {
        if (!NitroxPrefs.WantAchievements.Value || !GameModeUtils.AllowsAchievements())
        {
            return false;
        }

        bool allowed = Resolve<LocalPlayer>().AchievementsMode switch
        {
            AchievementsMode.NO_ACHIEVEMENTS => false,
            AchievementsMode.ACHIEVEMENTS_UNLESS_CHEATING => !DevConsole.HasUsedConsole(),
            AchievementsMode.ACHIEVEMENTS_WITH_CHEATING => true,
            _ => true
        };
        if (!allowed)
        {
            return false;
        }

        PlatformUtils.main.GetServices().UnlockAchievement(id);
        return false;
    }
}
