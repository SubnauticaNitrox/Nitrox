using System.Reflection;
using Harmony;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Client
{
    class IHandTarget_PilotingChair_Patch : IHandTarget_Generic_Patch<PilotingChair>
    {
        // TODO: When cinematicsmode things are not used anyway, why not just implment IHandTarget?

        public static readonly MethodInfo TARGET_METHOD_OnSteeringStart = TARGET_CLASS.GetMethod("OnSteeringStart", BindingFlags.NonPublic | BindingFlags.Instance);
        public override void Patch(HarmonyInstance harmony)
        {
            // Disable OnHandClick patch, as it's not overridden by PilotingChair. Instead, delegate the OnSteeringStart call to it.
            // Problem is, that it's called by onCinematicEnd, so after the player has done the animation.

            // TODO: Clean up this mess. Initially it was a nice idea but at that point I didn't know OnHandClick wasn't overridden everywhere, now the code is just ugly.
            // So instead make the OnHandClick_Prefix method more different/generic so that it looks a bit nicer.

            PatchPrefix(harmony, TARGET_METHOD_OnSteeringStart, "OnSteeringStart_Prefix");
            base.Patch(harmony);
        }

        public static bool OnSteeringStart_Prefix(PilotingChair __instance, CinematicModeEventData eventData)
        {
            return TryGetOwnershipBeforeProceeding(__instance, eventData);
        }

        protected override void OnReceiveOwnership(PilotingChair __instance, object data)
        {
            TARGET_METHOD_OnSteeringStart.Invoke(__instance, new[] { data });
        }

        protected override Optional<string> GetGuid(PilotingChair instance)
        {
            SubRoot subRoot = instance.GetComponentInParent<SubRoot>();
            Validate.NotNull(subRoot, "PilotingChair cannot find it's corresponding SubRoot!");
            string guid = GuidHelper.GetGuid(subRoot);
            return Optional<string>.Of(guid);
        }

        protected override void HasOwnership(PilotingChair instance, string guid)
        {
            // Nothing to do for now; other clients respond to VehicleMovement and update state accordingly.
        }

        protected override void OnHandDeny(PilotingChair instance, string guid)
        {
            // TODO: Localized strings?
            // TODO: Is the PilotingChair on more than just the Cyclops?
            HandReticle.main.SetInteractText("Another player is currently steering the Cyclops!");
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);

            // OnHandClick cannot be patched, and OnSteeringStart is fired after playing the animation.
            // The only racecondition this causes is that the player may see the animation and then end up not controlling the vehicle because another player was a little faster.
            // IDEA: patch Start() and hook in on onCinematicStart.
            instance.isValidHandTarget = false;
        }
    }
}
