using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

/// <summary>
///     Add/remove <see cref="CyclopsDamagePoint" />s and <see cref="Fire" />s to match the
///     <see cref="CyclopsDamagePointCreated" />
///     packet received
/// </summary>
internal sealed class CyclopsDamagePointCreatedProcessor : IClientPacketProcessor<CyclopsDamagePointCreated>
{
    public Task Process(ClientProcessorContext context, CyclopsDamagePointCreated packet)
    {
        CyclopsExternalDamageManager damageManager = NitroxEntity.RequireObjectFrom(packet.Id).RequireComponent<SubRoot>().damageManager;
        CyclopsDamagePoint damagePoint = damageManager.damagePoints[packet.DamagePointIndex];

        if (!damagePoint.gameObject.activeSelf)
        {
            // Copied from CyclopsExternalDamageManager.CreatePoint(), except without the random index pick.
            damagePoint.gameObject.SetActive(true);
            damagePoint.RestoreHealth();
            GameObject prefabGo = damageManager.fxPrefabs[Random.Range(0, damageManager.fxPrefabs.Length)];
            damagePoint.SpawnFx(prefabGo);
            damageManager.unusedDamagePoints.Remove(damagePoint);
        }

        // Visual update only to show the water leaking through the window and various hull points based on missing health.
        damageManager.ToggleLeakPointsBasedOnDamage();

        return Task.CompletedTask;
    }
}
