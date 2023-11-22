using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class uGUI_OnApplicationQuit_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((uGUI t) => t.OnApplicationQuit());

    public static void Prefix()
    {
        Resolve<IMultiplayerSession>().Disconnect();
    }
}
