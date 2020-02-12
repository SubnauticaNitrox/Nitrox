using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PowerLevelChangedProcessor : ClientPacketProcessor<PowerLevelChanged>
    {
        public override void Process(PowerLevelChanged packet)
        {
            GameObject gameObject = NitroxEntity.RequireObjectFrom(packet.Id);

            if (packet.PowerType == PowerType.ENERGY_INTERFACE)
            {
                EnergyInterface energyInterface = gameObject.RequireComponent<EnergyInterface>();

                float amount = packet.Amount;
                float num = 0f;
                if (GameModeUtils.RequiresPower())
                {
                    int num2 = 0;
                    if (packet.Amount > 0f)
                    {
                        float num3 = energyInterface.TotalCanConsume(out num2);
                        if (num3 > 0f)
                        {
                            float amount2 = amount / (float)num2;
                            for (int i = 0; i < energyInterface.sources.Length; i++)
                            {
                                EnergyMixin energyMixin = energyInterface.sources[i];
                                if (energyMixin != null && energyMixin.charge < energyMixin.capacity)
                                {
                                    num += energyMixin.ModifyCharge(amount2);
                                }
                            }
                        }
                    }
                    else
                    {
                        float num4 = energyInterface.TotalCanProvide(out num2);
                        if (num2 > 0)
                        {
                            amount = ((-amount <= num4) ? amount : (-num4));
                            for (int j = 0; j < energyInterface.sources.Length; j++)
                            {
                                EnergyMixin energyMixin2 = energyInterface.sources[j];
                                if (energyMixin2 != null && energyMixin2.charge > 0f)
                                {
                                    float num5 = energyMixin2.charge / num4;
                                    num += energyMixin2.ModifyCharge(amount * num5);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                Log2.Instance.Log(NLogType.Error, "Unsupported packet power type: " + packet.PowerType);
            }
        }
    }
}
