using System.Reflection;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class EscapePod_StopIntroCinematic_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo targetMethod = Reflect.Method((EscapePod e) => e.StopIntroCinematic(default(bool)));

    public static void Postfix(bool isInterrupted)
    {
        if (isInterrupted)
        {
            //TODO: Currently this is sent as an unauthenticated packet when skipping the intro up front (Configuration=Debug) and is therefore not processed by the server.
            Resolve<PlayerCinematics>().SetLocalIntroCinematicMode(IntroCinematicMode.COMPLETED);
        }
    }
}
