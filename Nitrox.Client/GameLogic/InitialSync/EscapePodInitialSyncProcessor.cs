using System.Collections;
using Nitrox.Client.GameLogic.InitialSync.Base;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.GameLogic.InitialSync
{
    public class EscapePodInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodInitialSyncProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            EscapePodModel escapePod = packet.EscapePodsData.Find(x => x.Id.Equals(packet.AssignedEscapePodId));
            
            escapePodManager.AssignPlayerToEscapePod(escapePod, packet.FirstTimeConnecting);
            yield return null;

            escapePodManager.SyncEscapePodIds(packet.EscapePodsData);
        }
    }
}
