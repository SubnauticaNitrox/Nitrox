using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts a beacon rename from BeaconLabel.SetLabel(string) rather than BeaconLabel.OnHandClick.
///
/// The previous version patched OnHandClick with a postfix that registered a one-shot callback on
/// uGUI.main.userInput.callback, to fire once the player finished typing. That's the flat/desktop
/// path only: BeaconLabel.OnHandClick's vanilla body calls uGUI.main.userInput.RequestString(...,
/// SetLabel) to open the desktop naming dialog, whose callback eventually invokes SetLabel. But for
/// VR, SubmersedVR's own ShowVirtualKeyboardOnBeacon patch is a PREFIX on OnHandClick that returns
/// false -- skipping that vanilla body (and the RequestString call) entirely -- and instead opens
/// its own SteamVR keyboard via VirtualKeyboard.OpenKeyboardWithText, whose completion callback
/// calls BeaconLabel.SetLabel(label) directly. uGUI.main.userInput.callback is never touched by that
/// path, so the old postfix's registered handler just sits there unfired: VR beacon renames update
/// locally (SetLabel still runs and updates the visible label) but never broadcast at all.
///
/// SetLabel(string) is the one method both paths actually call -- RequestString's callback
/// parameter *is* SetLabel (BeaconLabel.OnHandClick: "RequestString(..., SetLabel)"), and
/// VirtualKeyboard's VR callback calls it directly -- so it's the right place to trigger the
/// broadcast from, uniformly, instead of chasing each path's own click/deselect trigger.
/// </summary>
public sealed partial class BeaconLabel_SetLabel_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((BeaconLabel t) => t.SetLabel(default));

    public static void Postfix(BeaconLabel __instance)
    {
        if (__instance.transform.parent && __instance.transform.parent.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<Entities>().EntityMetadataChanged(__instance.transform.parent.GetComponent<Beacon>(), id);
        }
    }
}
