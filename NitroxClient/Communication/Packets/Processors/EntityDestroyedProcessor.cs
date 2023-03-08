using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors;

public class EntityDestroyedProcessor : ClientPacketProcessor<EntityDestroyed>
{
    public const DamageType DAMAGE_TYPE_RUN_ORIGINAL = (DamageType)100;

    private readonly Entities entities;
    private readonly IPacketSender packetSender;

    public EntityDestroyedProcessor(Entities entities, IPacketSender packetSender)
    {
        this.entities = entities;
        this.packetSender = packetSender;
    }

    public override void Process(EntityDestroyed packet)
    {
        entities.RemoveEntity(packet.Id);

        using (PacketSuppressor<EntityDestroyed>.Suppress())
        {
            if (NitroxEntity.TryGetObjectFrom(packet.Id, out GameObject gameObject))
            {
                // This type of check could get out of control if there are many types with custom destroy logic.  If we get a few more, move to separate processors.
                Vehicle vehicle = gameObject.GetComponent<Vehicle>();
                SubRoot subRoot = gameObject.GetComponent<SubRoot>();

                if (vehicle)
                {
                    DestroyVehicle(vehicle);
                }
                else if(subRoot)
                {
                    DestroySubroot(subRoot);
                }
                else
                {
                    DefaultDestroyAction(gameObject);
                }
            }
        }
    }

    private void DestroySubroot(SubRoot subRoot)
    {
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

    private void DestroyVehicle(Vehicle vehicle)
    {
        if (vehicle.GetPilotingMode()) //Check Local Object Have Player inside
        {
            vehicle.OnPilotModeEnd();

            if (!Player.main.ToNormalMode(true))
            {
                Player.main.ToNormalMode(false);
                Player.main.transform.parent = null;
            }
        }

        foreach (RemotePlayerIdentifier identifier in vehicle.GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            identifier.RemotePlayer.ResetStates();
        }

        if (vehicle.gameObject)
        {
            if (vehicle.destructionEffect)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(vehicle.destructionEffect);
                gameObject.transform.position = vehicle.transform.position;
                gameObject.transform.rotation = vehicle.transform.rotation;
            }

            UnityEngine.Object.Destroy(vehicle.gameObject);
        }
    }

    private void DefaultDestroyAction(GameObject gameObject)
    {
        UnityEngine.Object.Destroy(gameObject);
    }
}
