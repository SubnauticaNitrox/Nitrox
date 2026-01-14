using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class StoryHandTarget_OnHandClick_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryHandTarget t) => t.OnHandClick(default(GUIHand)));

    public static void Postfix(StoryHandTarget __instance)
    {
        if (__instance.destroyGameObject == __instance.gameObject &&
            __instance.gameObject.TryGetNitroxId(out NitroxId destroyId))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(destroyId));
            return;
        }

        if (__instance.informGameObject)
        {
            if (__instance.informGameObject.TryGetComponent(out PrecursorComputerTerminal terminal) &&
                __instance.informGameObject.TryGetNitroxId(out NitroxId terminalId))
            {
                Resolve<Entities>().BroadcastMetadataUpdate(terminalId, new PrecursorComputerTerminalMetadata(terminal.used));
            }
            else if (__instance.informGameObject.TryGetComponent(out GenericConsole console) &&
                     __instance.informGameObject.TryGetNitroxId(out NitroxId consoleId))
            {
                Resolve<Entities>().BroadcastMetadataUpdate(consoleId, new GenericConsoleMetadata(console.gotUsed));
            }
        }
    }
}
