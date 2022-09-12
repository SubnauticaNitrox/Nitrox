using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public class Base_SetFace_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((Base t) => t.SetFace(default, default));

    public static void Prefix(Base __instance, Base.Face face, Base.FaceType faceType)
    {
        // We don't want to track events that will happen during BuildingInitialSync or certain other defined times
        if (!Base_CopyFrom_Patch.TrackDeletedFaces)
        {
            return;
        }

        // We won't handle calls from other places than Base.CopyFrom()
        if (!Base_CopyFrom_Patch.DeletedFaces.ContainsKey(__instance))
        {
            return;
        }

        int index = __instance.baseShape.GetIndex(face.cell);
        if (index == -1)
        {
            return;
        }

        // It's the only type of replacement we can detect
        if (faceType != Base.FaceType.None)
        {
            return;
        }

        Base.FaceType oldFaceType = __instance.faces[__instance.GetNormalizedFaceIndex(index, face.direction)];
        bool replaced = false;
        // List of FaceTypes that could be replaced by a corridor
        switch (oldFaceType)
        {
            case Base.FaceType.Window:
            case Base.FaceType.Hatch:
            case Base.FaceType.Reinforcement:
            case Base.FaceType.Planter:
            case Base.FaceType.FiltrationMachine:
            case Base.FaceType.UpgradeConsole:
                replaced = true;
                break;
        }

        if (replaced)
        {
            // TOREMOVE: Log.Debug($"SetFace({face},{faceType}) {oldFaceType} replaced index: {index}");
            GameObject faceObject = FindFaceObject(__instance, face, oldFaceType);
            if (faceObject)
            {
                // TOREMOVE: Log.Debug($"Found an object corresponding to the face: {faceObject.name}");
                if (NitroxEntity.TryGetEntityFrom(faceObject, out NitroxEntity entity))
                {
                    // To make sure we don't accidentally delete some part, we'll add another check
                    Base_CopyFrom_Patch.DeletedFaces[__instance].Add(entity.Id);
                }
            }
            else
            {
                Log.Warn($"Corresponding object to the face {face} could not be found");
            }
        }
    }

    private static GameObject FindFaceObject(Base @base, Base.Face face, Base.FaceType faceType, bool retry = true)
    {
        // TOREMOVE: Log.Debug($"FindFaceObject({face},{retry})");
        // TOREMOVE: Log.Debug($"Base's anchor: {@base.GetAnchor()}");

        // Pretty much the same loop as in Base.BindCellObjects but we need to do it this way
        foreach (BaseCell baseCell in @base.GetComponentsInChildren<BaseCell>(true))
        {
            // TOREMOVE: Log.Debug($"Looking inside baseCell: {baseCell.name}");
            if (baseCell.transform.parent == @base.transform)
            {
                Int3 @int = @base.WorldToGrid(baseCell.transform.position);
                int index = @base.baseShape.GetIndex(@int);
                if (index == -1)
                {
                    continue;
                }
                foreach (BaseDeconstructable deconstructable in baseCell.GetComponentsInChildren<BaseDeconstructable>(true))
                {
                    // TOREMOVE: Log.Debug($"Found BaseDeconstructable: {deconstructable.name}, face: {deconstructable.face}, bounds: {deconstructable.bounds}");
                    if (deconstructable.face.HasValue &&
                        deconstructable.face.Value == face &&
                        deconstructable.faceType == faceType)
                    {
                        return deconstructable.gameObject;
                    }
                }
            }
        }

        if (retry)
        {
            // TOREMOVE: Log.Debug("FindFaceObject failed, will retry with the adjacent face");
            // If we don't get it the first try (probably because the face direction is West or South), we'll just retry once with the adjacent cell
            return FindFaceObject(@base, new(Base.GetAdjacent(face.cell, face.direction), face.direction), faceType, false);
        }
        return null;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
