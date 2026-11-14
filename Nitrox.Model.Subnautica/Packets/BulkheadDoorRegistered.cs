using System;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
/// Gives a base's bulkhead door its server-owned entity. Sent by the client that toggles a door which doesn't have an entity yet,
/// and sent back by the server (to the sender only) when another entity already exists for that face.
/// </summary>
[Serializable]
public sealed class BulkheadDoorRegistered : Packet
{
    public BulkheadDoorEntity Entity { get; }

    public BulkheadDoorRegistered(BulkheadDoorEntity entity)
    {
        Entity = entity;
    }
}
