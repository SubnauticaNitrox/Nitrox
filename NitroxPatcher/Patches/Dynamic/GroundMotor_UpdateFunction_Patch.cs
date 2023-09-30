using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class GroundMotor_UpdateFunction_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((GroundMotor t) => t.UpdateFunction());

    public static void Postfix(GroundMotor __instance)
    {
        if (Physics.Raycast(__instance.transform.position + __instance.controller.center + Vector3.down * __instance.controller.height * 0.5f, Vector3.down, out RaycastHit hitInfo, __instance.controller.skinWidth + 0.01f) && hitInfo.transform.TryGetComponent(out Rigidbody rb))
        {
            Vector3 platformPosition = hitInfo.transform.position;
            Vector3 rbPosition = rb.position;

            Vector3 posRelativeToRigidbody = __instance.transform.position - rbPosition;
            Vector3 posRelativeToPlatform = posRelativeToRigidbody + platformPosition;

            __instance.transform.position = posRelativeToPlatform;
        }
    }
}
