using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Replaces "fasthatch" command by the Nitrox server command implementation
/// </summary>
public sealed partial class NoCostConsoleCommand_OnConsoleCommand_fasthatch_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((NoCostConsoleCommand t) => t.OnConsoleCommand_fasthatch(default));

    public static bool Prefix()
    {
        bool newValue = !NoCostConsoleCommand.main.fastHatchCheat; // toggle is basic functionning
        Resolve<IPacketSender>().Send(new ServerCommand($"fast hatch {newValue}"));
        return false;
    }
}
