using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using System.Reflection;
using System;
using NitroxModel.Logger;
using NitroxModel_Subnautica.Packets;
using UnityEngine;


namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsActivateSonarProcessor : ClientPacketProcessor<CyclopsActivateSonar>
    {
        private readonly IPacketSender packetSender;

        public CyclopsActivateSonarProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsActivateSonar sonarPacket)
        {            
            GameObject cyclops = GuidHelper.RequireObjectFrom(sonarPacket.Guid);
            CyclopsSonarButton sonar = cyclops.RequireComponentInChildren<CyclopsSonarButton>();
            using (packetSender.Suppress<CyclopsActivateSonar>())
            {
                // At this moment the code is "non functional" as for some reason changing the sprite will never happen
                // Also setting sonar as active will never work
                //Log.Debug("Process sonar click guid: " + sonar.gameObject.GetGuid().ToString());
                MethodInfo sonarSetActiveInfo = sonar.GetType().GetMethod("set_sonarActive", BindingFlags.NonPublic | BindingFlags.Instance);
                if (sonarSetActiveInfo != null)
                {
                    sonarSetActiveInfo.Invoke(sonar, new object[] { sonarPacket.Active });
                }
                if (sonarPacket.Active)
                {
                    sonar.image.sprite = sonar.activeSprite;
                } else
                {
                    sonar.image.sprite = sonar.inactiveSprite;
                }
                //Log.Debug("Sonar onclick from sender processed");
            }
        }
    }
}
