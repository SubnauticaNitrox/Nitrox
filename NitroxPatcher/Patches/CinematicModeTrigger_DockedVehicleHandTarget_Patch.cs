using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Client
{
    class CinematicModeTrigger_DockedVehicleHandTarget_Patch : CinematicModeTrigger_Generic_Patch<DockedVehicleHandTarget>
    {
        protected override Optional<string> GetGuid(DockedVehicleHandTarget instance)
        {
            Vehicle dockedVehicle = instance.dockingBay.GetDockedVehicle();
            if (dockedVehicle)
            {
                return Optional<string>.Of(GuidHelper.GetGuid(dockedVehicle.gameObject));
            }
            return Optional<string>.Empty();
        }

        protected override void HasOwnership(DockedVehicleHandTarget instance, string guid)
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            IMultiplayerSessionState multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSessionState>();

            string subGuid = GuidHelper.GetGuid(instance.dockingBay.subRoot.gameObject);
            Log.Debug("Sending UndockingInitiate");
            packetSender.Send(new VehicleDocking(multiplayerSession.Reservation.PlayerId, subGuid, guid, DockingAction.UndockingInitiate));
        }

        protected override void OnHandDeny(DockedVehicleHandTarget instance, string guid)
        {
            // TODO: Localized strings?
            HandReticle.main.SetInteractText("Another player is currently in this vehicle!");
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }

        protected override void OnPlayerCinematicModeEnd(DockedVehicleHandTarget instance, string guid)
        {
            IPacketSender packetSender = NitroxServiceLocator.LocateService<IPacketSender>();
            IMultiplayerSessionState multiplayerSession = NitroxServiceLocator.LocateService<IMultiplayerSessionState>();

            string subGuid = GuidHelper.GetGuid(instance.dockingBay.subRoot.gameObject);
            packetSender.Send(new VehicleDocking(multiplayerSession.Reservation.PlayerId, subGuid, guid, DockingAction.UndockingPlayerIn));
        }
    }
}
