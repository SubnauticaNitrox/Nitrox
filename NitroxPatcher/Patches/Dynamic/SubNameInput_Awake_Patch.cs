using System.Reflection;
using Nitrox.Model.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <inheritdoc cref="SubNameInput_SetTarget_Patch"/>
public sealed class SubNameInput_Awake_Patch : PacketSuppressorPatch<EntityMetadataUpdate>, IDynamicPatch
{
    public override MethodInfo TARGET_METHOD => Reflect.Method((SubNameInput t) => t.Awake());
}
