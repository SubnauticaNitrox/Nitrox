using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class Base_CopyFrom_Patch : NitroxPatch, IDynamicPatch
{
    public readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.CopyFrom(default, default, default));

    // TODO: In the future, if faces are directly manipulated, we need to make sure that TrackDeletedFaces is accordingly set
    public static bool TrackDeletedFaces => Multiplayer.Main.InitialSyncCompleted;
    public static Dictionary<Base, List<NitroxId>> DeletedFaces = new();

    public static void Prefix(Base __instance, Base sourceBase)
    {
        NitroxEntity entity = sourceBase.GetComponent<NitroxEntity>();

        // The game will clone the base when doing things like rebuilding gemometry or placing a new
        // piece.  The copy is normally between a base ghost and a base - and vise versa.  When building
        // a face piece, such as a window, this will clone a ghost base to stage the change which is later
        // integrated into the real base.  For now, prevent guid copies to these staging ghost bases; however,
        // there is still a pending edge case when a base converts to a BaseGhost for deconstruction.
        if (entity != null && __instance.gameObject.name != "BaseGhost")
        {
            Log.Debug("Transfering base id : " + entity.Id + " from " + sourceBase.name + " to " + __instance.name);
            NitroxEntity.SetNewId(__instance.gameObject, entity.Id);
        }

        if (TrackDeletedFaces)
        {
            DeletedFaces[__instance] = new();
        }
    }

    public static void Postfix(Base __instance)
    {
        if (!DeletedFaces.TryGetValue(__instance, out List<NitroxId> deletedFaces))
        {
            return;
        }

        // At the end of the base rebuilding, we'll make sure that the deleted stuff is for sure deleted
        foreach (NitroxId id in deletedFaces)
        {
            if (!NitroxEntity.TryGetObjectFrom(id, out GameObject faceObject) || !faceObject || !faceObject.transform.parent)
            {
                // We can assume that the object was successfully deleted from now
                SendPacket<DeconstructionCompleted>(new(id, true));
                Log.Debug($"Face object with the id {id} was successfully deleted");
            }
            else
            {
                Log.Debug($"Falsely detected a deleted face object: {id}");
            }
        }

        DeletedFaces[__instance] = null;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
