using System.Reflection;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Suppresses <see cref="EntityMetadataUpdate"/> packets to avoid unexpected name and colors metadata updates.
/// </summary>
public sealed class SubNameInput_SetTarget_Patch : PacketSuppressorPatch<EntityMetadataUpdate>, IDynamicPatch
{
    public override MethodInfo TARGET_METHOD => Reflect.Method((SubNameInput t) => t.SetTarget(default));
}
