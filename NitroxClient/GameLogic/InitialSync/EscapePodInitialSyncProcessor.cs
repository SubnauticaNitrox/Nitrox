using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.InitialSync
{
    public class EscapePodInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodInitialSyncProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override void Process(InitialPlayerSync packet)
        {
            EscapePodModel escapePod = packet.EscapePodsData.Find(x => x.Guid == packet.AssignedEscapePodGuid);
            escapePodManager.AssignPlayerToEscapePod(escapePod);
            escapePodManager.SyncEscapePodGuids(packet.EscapePodsData);
        }
    }
}
