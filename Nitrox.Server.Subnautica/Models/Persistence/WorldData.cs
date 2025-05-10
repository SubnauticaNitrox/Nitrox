using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;

namespace Nitrox.Server.Subnautica.Models.Persistence;

[DataContract]
internal record WorldData
{
    [DataMember(Order = 1)]
    public HashSet<NitroxInt3> ParsedBatchCells { get; set; }

    public bool IsValid()
    {
        return ParsedBatchCells != null; // Always returns false on empty saves (sometimes also if never entered the ocean);
    }
}
