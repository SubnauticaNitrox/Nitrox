using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Adds <see cref="CyclopsMotor"/> to the Player's object when it's initialized
/// </summary>
public sealed partial class Player_Start_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Player t) => t.Start());

    public static void Postfix()
    {
        if (Player.mainObject.GetComponent<CyclopsMotor>())
        {
            return;
        }
        GroundMotor groundMotor = Player.mainObject.GetComponent<GroundMotor>();
        Player.mainObject.AddComponent<CyclopsMotor>().Initialize(groundMotor);
    }
}
