// Set "false" to "true" then start any game and look into game.log to find the code to paste in ReefbackSpawnData.cs
#if false
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

public sealed partial class ReefbackLife_OnEnable_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ReefbackLife t) => t.OnEnable());

    private static bool printed;

    public static void Postfix(ReefbackLife __instance)
    {
        if (printed)
        {
            return;
        }
        printed = true;

        // Make sure to switch culture so that formatted string use "." instead of "," for numbers
        CultureInfo previousCulture = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        
        string text = $"\npublic const int CREATURE_SLOTS_COUNT = {__instance.creatureSlots.Length};";
        text += $"\npublic const int PLANT_SLOTS_COUNT = {__instance.plantSlots.Length};";
        text += $"\npublic const int GRASS_VARIANTS_COUNT = {__instance.grassVariants.Length};";
        text += $"\n\npublic static readonly NitroxTransform CreatureSlotsRootTransform = {TransformToString(__instance.creatureSlots[0].parent)};";
        text += $"\npublic static readonly NitroxTransform PlantSlotsRootTransform = {TransformToString(__instance.plantSlotsRoot)};";

        text += "\n\npublic static List<ReefbackSlotCreature> SpawnableCreatures { get; } =\n[\n";
        foreach (ReefbackSlotsData.ReefbackSlotCreature reefbackSlotCreature in __instance.reefbackSlotsData.creatures)
        {
            string classId = reefbackSlotCreature.prefab.GetComponent<UniqueIdentifier>().ClassId;
            text += $"new() {{ Probability = {reefbackSlotCreature.probability}f, MinNumber = {reefbackSlotCreature.minNumber}, MaxNumber = {reefbackSlotCreature.maxNumber}, ClassId = \"{classId}\" }},\n";
        }

        text += "];\n\npublic static List<ReefbackSlotPlant> SpawnablePlants { get; } =\n[\n";
        foreach (ReefbackSlotsData.ReefbackSlotPlant reefbackSlotPlant in __instance.reefbackSlotsData.plants)
        {
            List<string> list = [];
            foreach (GameObject gameObject in reefbackSlotPlant.prefabVariants)
            {
                list.Add(gameObject.GetComponent<UniqueIdentifier>().ClassId);
            }
            text += $"new() {{ ClassIds = [\"{string.Join("\", \"", list)}\"], Probability = {reefbackSlotPlant.probability}f, StartRotation = {Vector3ToString(reefbackSlotPlant.startRotation)} }},\n";
        }

        text += "];\n\npublic static List<NitroxTransform> CreatureSlotsCoordinates { get; } =\n[\n";
        foreach (Transform transform in __instance.creatureSlots)
        {
            text += $"{TransformToString(transform)},\n";
        }

        text += "];\n\npublic static List<NitroxTransform> PlantSlotsCoordinates { get; } =\n[\n";
        foreach (Transform transform in __instance.plantSlots)
        {
            text += $"{TransformToString(transform)},\n";
        }
        text += "];\n";

        Log.Info(text);

        CultureInfo.CurrentCulture = previousCulture;
    }

    private static string TransformToString(Transform transform)
    {
        return $"new({Vector3ToString(transform.localPosition)}, {QuaternionToString(transform.localRotation)}, {Vector3ToString(transform.localScale)})";
    }

    private static string Vector3ToString(Vector3 vector3)
    {
        return $"new({vector3.x}f, {vector3.y}f, {vector3.z}f)";
    }

    private static string QuaternionToString(Quaternion quaternion)
    {
        return $"new({quaternion.x}f, {quaternion.y}f, {quaternion.z}f, {quaternion.w}f)";
    }
}
#endif
