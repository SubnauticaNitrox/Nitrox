using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Serialization.World;

internal class WorldData
{
    public List<NitroxInt3>? ParsedBatchCells { get; set; } = [];

    public GameData? GameData { get; set; }

    [Obsolete("Use server.cfg seed instead - TODO: delete this but keep backward compat via save upgrade")]
    public string? Seed { get; set; }

    public bool IsValid()
    {
        return ParsedBatchCells != null &&
               GameData != null;
    }
}
