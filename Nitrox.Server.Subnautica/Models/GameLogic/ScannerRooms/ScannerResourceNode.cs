using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal readonly record struct ScannerResourceNodeKey(NitroxId EntityId, ushort TrackerIndex);

internal readonly record struct ScannerResourceNode(
    ScannerResourceNodeKey Key,
    NitroxTechType TechType,
    NitroxVector3 Position,
    NitroxInt3 BatchId);
