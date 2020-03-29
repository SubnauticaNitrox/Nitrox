using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsDestroyedProcessor : ClientPacketProcessor<CyclopsDestroyed>
    {
        public override void Process(CyclopsDestroyed packet)
        {
            Optional<GameObject> cyclops = NitroxEntity.GetObjectFrom(packet.Id);

            // Despite me telling it to kill itself when I call LiveMixin.TakeDamage with massive damage, it doesn't seem to trigger like it should. 
            // To get around this problem, I manually set the health to 0, then call for Kill(). Since Kill() does not trigger the necessary events, I do
            // it all manually below. At least this way I can make the screen shake hard enough to cause motion sickness, or disable it if I wanted to.
            if (cyclops.HasValue)
            {
                Log.Debug("[CyclopsDestroyedProcessor Id: " + packet.Id + "]");

                SubRoot subRoot = cyclops.Value.RequireComponent<SubRoot>();

                cyclops.Value.RequireComponent<LiveMixin>().ReflectionSet("health", 0f, true, false);
                cyclops.Value.RequireComponent<LiveMixin>().Kill();

                // Copied from SubRoot.OnTakeDamage(DamageInfo info)
                subRoot.voiceNotificationManager.ClearQueue();
                subRoot.voiceNotificationManager.PlayVoiceNotification(subRoot.abandonShipNotification, false, true);
                subRoot.Invoke("PowerDownCyclops", 13f);
                subRoot.Invoke("DestroyCyclopsSubRoot", 18f);
                float num = Vector3.Distance(subRoot.transform.position, Player.main.transform.position);
                if (num < 20f)
                {
                    MainCameraControl.main.ShakeCamera(1.5f, 20f, MainCameraControl.ShakeMode.Linear, 1f);
                }
                if (subRoot.damageManager != null)
                {
                    subRoot.damageManager.SendMessage("OnTakeDamage", null, SendMessageOptions.DontRequireReceiver);
                }
            }
        }
    }
}
