using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    /// <summary>
    /// The Cyclops damage logic is handled in 3 spots. <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>, <see cref="CyclopsExternalDamageManager.OnTakeDamage"/>,
    /// and if it has the shield modification installed, <see cref="LiveMixin.TakeDamage(float, Vector3, DamageType, GameObject)"/>.
    /// Repair Logic is handled at <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> and <see cref="CyclopsDamagePoint.OnRepair"/>.
    /// 
    /// Currently it does a full re-sync of damage points. Will likely be re-written to handle add/remove events, and in the rare case, a full re-sync request when
    /// we choose if/how we will sync the random number generator.
    /// </summary>
    public class CyclopsDamageProcessor : ClientPacketProcessor<CyclopsDamage>
    {
        private readonly IPacketSender packetSender;

        public CyclopsDamageProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(CyclopsDamage packet)
        {
            GameObject cyclops = GuidHelper.RequireObjectFrom(packet.Guid);
            CyclopsExternalDamageManager damageManager = cyclops.RequireComponentInChildren<CyclopsExternalDamageManager>();

            List<CyclopsDamagePoint> unusedDamagePoints = (List<CyclopsDamagePoint>)damageManager.ReflectionGet("unusedDamagePoints");
            //FieldInfo unusedDamagePointsField = typeof(CyclopsExternalDamageManager).GetField("unusedDamagePoints", BindingFlags.NonPublic | BindingFlags.Instance);
            //Validate.NotNull(unusedDamagePointsField, "Could not find 'unusedDamagePoints' method in class 'CyclopsExternalDamageManager'");
            //List<CyclopsDamagePoint> unusedDamagePoints = (List<CyclopsDamagePoint>)unusedDamagePointsField.GetValue(damageManager);

            // Sync the health of the Cyclops. The health of the Cyclops is checked when damaged/healed, and if there's a mismatch in the expected count of damage points,
            // it will add/remove them randomly until it matches the expected number. You can see this logic in CyclopsExternalDamageManager.OnTakeDamage().
            //damageManager.subLiveMixin.health = packet.Health;

            // CyclopsExternalDamageManager.damagePoints is an unchanged list. It will never have items added/removed from it. Since packet.DamagePointIndexes is also an array
            // generated in an ordered manner, we can match them without worrying about unordered items.
            if (packet.DamagePointIndexes != null && packet.DamagePointIndexes.Length > 0)
            {
                int packetDamagePointsIndex = 0;

                for (int damagePointsIndex = 0; damagePointsIndex < damageManager.damagePoints.Length; damagePointsIndex++)
                {
                    // Loop over all of the packet.DamagePointIndexes as long as there's more to match
                    if (packetDamagePointsIndex < packet.DamagePointIndexes.Length
                        && packet.DamagePointIndexes[packetDamagePointsIndex] == damagePointsIndex)
                    {
                        // Must be an added damage point. There's no method to activate a specific damage point, so we have to do it manually.
                        if (!damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            // Copied from CyclopsExternalDamageManager.CreatePoint(), except without the random index.
                            damageManager.damagePoints[damagePointsIndex].gameObject.SetActive(true);
                            damageManager.damagePoints[damagePointsIndex].RestoreHealth();
                            GameObject prefabGo = damageManager.fxPrefabs[UnityEngine.Random.Range(0, damageManager.fxPrefabs.Length - 1)];
                            damageManager.damagePoints[damagePointsIndex].SpawnFx(prefabGo);
                            unusedDamagePoints.Remove(damageManager.damagePoints[damagePointsIndex]);

                            Log.Debug("[CyclopsDamageProcessor Creating DamagePoint index: " + damagePointsIndex.ToString()
                                + " all packet DamagePoint Indexes: " + string.Join(", ", packet.DamagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");
                        }

                        packetDamagePointsIndex++;
                    }
                    else
                    {
                        // If it's active, but not in the list, it must have been repaired.
                        if (damageManager.damagePoints[damagePointsIndex].gameObject.activeSelf)
                        {
                            Log.Debug("[CyclopsDamageProcessor Repairing DamagePoint index: " + damagePointsIndex.ToString()
                                + " all packet DamagePoint Indexes: " + string.Join(", ", packet.DamagePointIndexes.Select(x => x.ToString()).ToArray()) + "]");

                            RepairPoint(damageManager, unusedDamagePoints, damagePointsIndex);
                        }
                    }
                }

                if (packetDamagePointsIndex < packet.DamagePointIndexes.Length)
                {
                    Log.Error("[CyclopsDamageProcessor packet.DamagePointGuids did not fully iterate! Guid: " + packet.DamagePointIndexes[packetDamagePointsIndex].ToString() 
                        + " had no matching Guid in damageManager.damagePoints, or the order is incorrect!]");
                }
            }
            else
            {
                Log.Debug("[CyclopsDamageProcessor No DamagePoints in packet, repairing all DamagePoints]");

                // None should be active.
                for (int i = 0; i < damageManager.damagePoints.Length; i++)
                {
                    if (damageManager.damagePoints[i].gameObject.activeSelf)
                    {
                        RepairPoint(damageManager, unusedDamagePoints, i);
                    }
                }
            }

            // Get that screen shake going on
            //if (packet.DamageInfo.Damage > 0)
            //{
            //    damageManager.subRoot.OnTakeDamage(new DamageInfo() { damage = packet.AttackDamage });
            //}

            // unusedDamagePoints is checked against damagePoints to determine if there's enough damage points. Failing to set the new list
            // of unusedDamagePoints will cause random DamagePoints to appear.
            damageManager.ReflectionSet("unusedDamagePoints", unusedDamagePoints);
            cyclops.GetComponent<LiveMixin>().health = packet.SubHealth;
            damageManager.subLiveMixin.health = packet.DamageManagerHealth;
            cyclops.GetComponent<SubFire>().liveMixin.health = packet.SubFireHealth;
            // ToggleLeakPointsBasedOnDamage is a visual update only.
            damageManager.ReflectionCall("ToggleLeakPointsBasedOnDamage", false, false, null);
        }

        /// <summary>
        /// Implements the logic for repairing a <see cref="CyclopsDamagePoint"/>. This is used over a direct call to 
        /// <see cref="CyclopsExternalDamageManager.RepairPoint(CyclopsDamagePoint)"/> because the original function would 
        /// trigger <see cref="NitroxPatcher.Patches.CyclopsDamagePoint_OnRepair_Patch"/>
        /// </summary>
        /// <param name="damageManager">The <see cref="CyclopsExternalDamageManager"/> located in <see cref="SubRoot.damageManager"/></param>
        /// <param name="unusedDamagePoints">A private variable located in <see cref="CyclopsExternalDamageManager"/></param>
        /// <param name="damagePointsIndex">The index of the <see cref="CyclopsDamagePoint"/></param>
        private void RepairPoint(CyclopsExternalDamageManager damageManager, List<CyclopsDamagePoint> unusedDamagePoints, int damagePointsIndex)
        {
            FieldInfo psField = typeof(CyclopsDamagePoint).GetField("ps", BindingFlags.NonPublic | BindingFlags.Instance);
            ParticleSystem ps = (ParticleSystem)psField.GetValue(damageManager.damagePoints[damagePointsIndex]);

            if (ps != null)
            {
                ps.transform.parent = null;
                ps.Stop();
                UnityEngine.Object.Destroy(ps.gameObject, 3f);
            }

            damageManager.damagePoints[damagePointsIndex].gameObject.SetActive(false);
            unusedDamagePoints.Add(damageManager.damagePoints[damagePointsIndex]);
        }
    }
}
