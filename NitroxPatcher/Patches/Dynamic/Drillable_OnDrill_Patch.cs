using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Spawning.Metadata;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Drillable_OnDrill_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Drillable t) => t.OnDrill(default, default, out Reflect.Ref<GameObject>.Field));

    public static void Postfix(Drillable __instance)
    {
        if (__instance.TryGetNitroxId(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance, id);
            return;
        }

        // For some reason the drillable ion cube deposit in the primary containment facility
        // is not automatically tagged as an entity. We spawn it here as a workaround.

        AnteChamber antechamber = __instance.GetComponentInParent<AnteChamber>();

        if (antechamber && antechamber.TryGetIdOrWarn(out NitroxId parentId))
        {
            id = NitroxEntity.GenerateNewId(__instance.gameObject);

            Optional<EntityMetadata> metadata = Resolve<EntityMetadataManager>().Extract(__instance.gameObject);
            Validate.IsPresent(metadata);

            PathBasedChildEntity entity = new(__instance.name, id, NitroxTechType.None, metadata.Value, parentId, []);
            Resolve<IPacketSender>().Send(new EntitySpawnedByClient(entity));
        }
    }
}
