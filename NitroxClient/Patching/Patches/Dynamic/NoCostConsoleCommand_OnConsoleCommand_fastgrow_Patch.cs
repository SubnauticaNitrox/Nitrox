using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Patching.Patches.Dynamic;

/// <summary>
/// Replaces "fastgrow" command by the Nitrox server command implementation
/// </summary>
public sealed partial class NoCostConsoleCommand_OnConsoleCommand_fastgrow_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((NoCostConsoleCommand t) => t.OnConsoleCommand_fastgrow(default));

    public static bool Prefix()
    {
        bool newValue = !NoCostConsoleCommand.main.fastGrowCheat; // toggle is basic functionning
        Resolve<IPacketSender>().Send(new ServerCommand($"fast grow {newValue}"));
        return false;
    }
}
