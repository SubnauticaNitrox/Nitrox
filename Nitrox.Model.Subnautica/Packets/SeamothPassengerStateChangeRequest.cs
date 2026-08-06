using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
///     Requests entering the given Seamoth as a passenger, or exiting when <see cref="SeamothId" /> is empty.
///     The server is the sole authority for assigning passenger seats.
/// </summary>
[Serializable]
public sealed class SeamothPassengerStateChangeRequest(Optional<NitroxId> seamothId) : Packet
{
    public Optional<NitroxId> SeamothId { get; } = seamothId;
}
