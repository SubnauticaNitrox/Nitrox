using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class LiveMixinManager
    {
        private readonly IMultiplayerSession multiplayerSession;
        public LiveMixinManager(IMultiplayerSession multiplayerSession)
        {
            this.multiplayerSession = multiplayerSession;
        }
        public void BroadcastTakeDamage(TechType techType, NitroxId id, float originalDamage, Vector3 position, DamageType damageType, Optional<NitroxId> dealerId, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, -originalDamage, position.ToDto(), (ushort)damageType, dealerId, totalHealth);
            multiplayerSession.Send(packet);
        }

        public void BroadcastAddHealth(TechType techType, NitroxId id, float healthAdded, float totalHealth)
        {
            LiveMixinHealthChanged packet = new LiveMixinHealthChanged(techType.ToDto(), id, healthAdded, totalHealth);
            multiplayerSession.Send(packet);
        }
    }
}
