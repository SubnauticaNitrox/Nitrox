using System.Collections.Generic;
using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Serialization.World
{
    [DataContract]
    internal class WorldData
    {
        [DataMember(Order = 1)]
        public List<NitroxInt3>? ParsedBatchCells { get; set; } = [];

        [DataMember(Order = 2)]
        public GameData? GameData { get; set; }

        [Obsolete("Use server.cfg seed instead - TODO: delete this but keep backward compat via save upgrade")]
        [DataMember(Order = 3)]
        public string? Seed { get; set; }

        public bool IsValid()
        {
            return ParsedBatchCells != null &&
                   GameData != null;
        }
    }
}
