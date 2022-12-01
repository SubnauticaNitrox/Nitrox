using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class CyclopsDestroyedProcessor : ClientPacketProcessor<CyclopsDestroyed>
{
    public const DamageType DAMAGE_TYPE_RUN_ORIGINAL = (DamageType)100;
    
    public override void Process(CyclopsDestroyed packet)
    {
        Optional<GameObject> cyclops = NitroxEntity.GetObjectFrom(packet.Id);
        if (!cyclops.HasValue)
        {
            return;
        }
        if (packet.Instantly)
        {
            cyclops.Value.RequireComponent<CyclopsDestructionEvent>().DestroyCyclops();
            return;
        }
        
        SubRoot subRoot = cyclops.Value.RequireComponent<SubRoot>();
        if (subRoot.live.health > 0f)
        {
            // oldHPPercent must be in the interval [0; 0.25[ because else, SubRoot.OnTakeDamage will end up in the wrong else condition
            subRoot.oldHPPercent = 0f;
            subRoot.live.health = 0f;
            subRoot.live.Kill();
        }

        // We use a specific DamageType so that the Prefix on this method will accept this call
        subRoot.OnTakeDamage(new() { type = DAMAGE_TYPE_RUN_ORIGINAL });
    }
}
