using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches;

public abstract class PacketSuppressorPatch<T> : NitroxPatch where T : Packet
{
    public abstract MethodInfo TARGET_METHOD { get; }
    private static PacketSuppressor<T> packetSuppressor;
    private static bool wasSuppressed;

    public static void Prefix()
    {
        wasSuppressed = PacketSuppressor<T>.IsSuppressed;
        packetSuppressor = PacketSuppressor<T>.Suppress();
    }

    public static void Finalizer()
    {
        if (!wasSuppressed)
        {
            packetSuppressor.Dispose();
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD, ((Action)Prefix).Method);
        PatchFinalizer(harmony, TARGET_METHOD, ((Action)Finalizer).Method);
    }
}
