using System.Collections.Generic;
using NitroxClient.GameLogic.Settings;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic;

public partial class Entities
{
    private readonly HashSet<NitroxId> requiredEntityResyncs = [];

    public int Requests;

    public void RequireResync(NitroxId nitroxId)
    {
        if (NitroxPrefs.EntitiesAutoResync.Value)
        {
            requiredEntityResyncs.Add(nitroxId);
            RequestEntityResyncs();
        }
    }

    public void RequestEntityResyncs()
    {
        if (spawningEntities || !NitroxPrefs.EntitiesAutoResync.Value || requiredEntityResyncs.Count == 0)
        {
            return;
        }
        Requests += requiredEntityResyncs.Count;
        packetSender.Send(new EntityResyncRequest([.. requiredEntityResyncs]));
        requiredEntityResyncs.Clear();
    }
}
