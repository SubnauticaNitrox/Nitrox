using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents "bobthebuilder" command from enabling "fasthatch" and "fastgrow", since both commands are managed by the server.
/// Thus, it attempts to enable the cheats that are currently disabled by sending the related command to the server.
/// </summary>
public sealed partial class NoCostConsoleCommand_OnConsoleCommand_bobthebuilder_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((NoCostConsoleCommand t) => t.OnConsoleCommand_bobthebuilder(default));

    public static void Prefix(out (bool, bool) __state)
    {
        __state = (NoCostConsoleCommand.main.fastHatchCheat, NoCostConsoleCommand.main.fastGrowCheat);
    }

    public static void Postfix((bool, bool) __state, NotificationCenter.Notification n)
    {
        NoCostConsoleCommand.main.fastHatchCheat = __state.Item1;
        NoCostConsoleCommand.main.fastGrowCheat = __state.Item2;
        
        // Requesting enabling of the cheats if they're currently disabled
        if (!__state.Item1)
        {
            NoCostConsoleCommand.main.OnConsoleCommand_fasthatch(n);
        }
        if (!__state.Item2)
        {
            NoCostConsoleCommand.main.OnConsoleCommand_fastgrow(n);
        }
    }
}
