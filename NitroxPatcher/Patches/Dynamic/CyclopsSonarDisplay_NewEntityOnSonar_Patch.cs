using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.DataStructures;
using NitroxPatcher.Patches.Persistent;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsSonarDisplay_NewEntityOnSonar_Patch : NitroxPatch, IPersistentPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsSonarDisplay t) => t.NewEntityOnSonar(default));

    /*
     * }
     * this.entitysOnSonar.Add(entityPing2);
     * SetupPing(component, entityData);        <----- INSERTED LINE
     */
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions).End()
                                            .InsertAndAdvance(TARGET_METHOD.Ldloc<CyclopsHUDSonarPing>())
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1))
                                            .InsertAndAdvance(new CodeInstruction(OpCodes.Call, Reflect.Method(() => SetupPing(default, default))))
                                            .InstructionEnumeration();
    }

    public static void SetupPing(CyclopsHUDSonarPing ping, CyclopsSonarCreatureDetector.EntityData entityData)
    {
        if (entityData.entityType != CyclopsSonarCreatureDetector_CheckForCreaturesInRange_Patch.PLAYER_TYPE)
        {
            return;
        }

        Color color;
        if (entityData.gameObject == Player.mainObject)
        {
            color = Resolve<LocalPlayer>().PlayerSettings.PlayerColor.ToUnity();
        }
        else if (entityData.gameObject.TryGetComponent(out RemotePlayerIdentifier remotePlayerIdentifier))
        {
            color = remotePlayerIdentifier.RemotePlayer.PlayerSettings.PlayerColor.ToUnity();
        }
        else
        {
            return;
        }

        CyclopsHUDSonarPing sonarPing = ping.GetComponent<CyclopsHUDSonarPing>();
        // Set isCreaturePing to true so that CyclopsHUDSonarPing.Start runs the SetColor code
        sonarPing.isCreaturePing = true;
        sonarPing.passiveColor = color;
        sonarPing.Start();
        sonarPing.isCreaturePing = false;

        // We remove the pulse to be able to differentiate those signals from the creatures and decoy ones
        GameObject.Destroy(sonarPing.transform.Find("Ping/PingPulse").gameObject);
    }
}
