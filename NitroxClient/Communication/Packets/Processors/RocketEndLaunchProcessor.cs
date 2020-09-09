using System.Collections;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class RocketEndLaunchProcessor : ClientPacketProcessor<RocketEndLaunch>
    {
        public override void Process(RocketEndLaunch packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);
            LaunchRocket launchRocket = gameObject.RequireComponentInChildren<LaunchRocket>(true);

            Log.InGame("Hope you enjoyed the Nitrox experience :)");

            launchRocket.ReflectionCall("SetLaunchStarted", false, true);
            PlayerTimeCapsule.main.Submit(null);

            IEnumerator endSequence = (IEnumerator)launchRocket.ReflectionCall("StartEndCinematic", false, false);
            launchRocket.StartCoroutine(endSequence);

            HandReticle.main.RequestCrosshairHide();
        }
    }
}
