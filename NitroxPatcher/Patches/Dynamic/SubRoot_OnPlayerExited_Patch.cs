using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class SubRoot_OnPlayerExited_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((SubRoot t) => t.OnPlayerExited(default(Player)));

    public static void Prefix()
    {
        GroundMotor motor = Player.main.groundMotor;
        motor.movingPlatform.movementTransfer = GroundMotor.MovementTransferOnJump.PermaTransfer;
    }
}
