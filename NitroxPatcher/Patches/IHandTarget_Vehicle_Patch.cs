using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;

namespace NitroxPatcher.Patches.Client
{
    class IHandTarget_Vehicle_Patch : IHandTarget_OnHandClick_Generic_Patch<Vehicle>
    {
        protected override Optional<string> GetGuid(Vehicle instance)
        {
            return Optional<string>.Of(GuidHelper.GetGuid(instance));
        }

        protected override void HasOwnership(Vehicle instance, string guid)
        {
            // Other clients should be able to figure this one out: Player is in Vehicle with exclusive ownertype.
        }

        protected override void OnHandDeny(Vehicle instance, string guid)
        {
            // TODO: Localized strings?
            // TODO: Is the PilotingChair on more than just the Cyclops?
            HandReticle.main.SetInteractText("Another player is currently in this Vehicle!");
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }
    }
}
