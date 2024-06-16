#if DEBUG
using System.Reflection;
using LiteNetLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class FPSCounter_UpdateDisplay_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((FPSCounter t) => t.UpdateDisplay());

    private static LiteNetLibClient client => Resolve<IClient>() as LiteNetLibClient;

    public static void Postfix(FPSCounter __instance)
    {
        if (!Multiplayer.Active)
        {
            return;
        }
        __instance.strBuffer.Append("Loading entities: ").AppendLine(Resolve<Entities>().EntitiesToSpawn.Count.ToString());
        __instance.strBuffer.Append("Real time elapsed: ").AppendLine(Resolve<TimeManager>().RealTimeElapsed.ToString());
        __instance.strBuffer.Append("Requested entity resyncs: ").AppendLine(Resolve<Entities>().Requests.ToString());
        if (client.IsConnected)
        {
            NetStatistics stats = client.GetStatistics();
            __instance.strBuffer.Append("Packet Loss: ").AppendLine($"{stats.PacketLoss} [{stats.PacketLossPercent}%]");
        }
        __instance.text.SetText(__instance.strBuffer);
    }
}
#endif
