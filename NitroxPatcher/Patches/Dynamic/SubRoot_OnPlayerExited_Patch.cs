using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubRoot_OnPlayerExited_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnPlayerExited(default(Player)));

    public static void Prefix()
    {
        PlayerMotor motor = Player.main.playerController.activeController;
        if (motor is GroundMotor groundMotor)
        {
            groundMotor.movingPlatform.movementTransfer = GroundMotor.MovementTransferOnJump.PermaTransfer;
        }
    }
}
