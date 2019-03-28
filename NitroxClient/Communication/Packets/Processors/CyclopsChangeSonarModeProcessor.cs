using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using System.Reflection;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;


namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsChangeSonarModeProcessor : ClientPacketProcessor<CyclopsChangeSonarMode>
    {
        private readonly IPacketSender packetSender;

        public CyclopsChangeSonarModeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsChangeSonarMode sonarPacket)
        {            
            GameObject cyclops = GuidHelper.RequireObjectFrom(sonarPacket.Guid);
            CyclopsSonarButton sonar = cyclops.RequireComponentInChildren<CyclopsSonarButton>();
            using (packetSender.Suppress<CyclopsChangeSonarMode>())
            {
                // At this moment the code is "non functional" as for some reason changing the sprite will never happen
                // Also setting sonar as active will never work
                
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
            }
        }
    }
}
