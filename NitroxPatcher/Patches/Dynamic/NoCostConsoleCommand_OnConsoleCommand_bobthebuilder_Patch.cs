using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents "bobthebuilder" command from enabling "fasthatch" and "fastgrow", since both commands are managed by the server
/// </summary>
public sealed partial class NoCostConsoleCommand_OnConsoleCommand_bobthebuilder_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((NoCostConsoleCommand t) => t.OnConsoleCommand_bobthebuilder(default));

    public static void Prefix(out (bool, bool) __state)
    {
        __state = (NoCostConsoleCommand.main.fastHatchCheat, NoCostConsoleCommand.main.fastGrowCheat);
        ErrorMessage.AddWarning("fastHatch and fastGrow cheats weren't enabled because they're shared cheat commands. Please enable them manually with their respective commands \"fastgrow\" and \"fasthatch\"");
    }

    public static void Postfix((bool, bool) __state)
    {
        NoCostConsoleCommand.main.fastHatchCheat = __state.Item1;
        NoCostConsoleCommand.main.fastGrowCheat = __state.Item2;
    }
}
