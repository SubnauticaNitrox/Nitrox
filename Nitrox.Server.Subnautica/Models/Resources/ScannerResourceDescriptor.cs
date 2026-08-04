using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Resources;

internal readonly record struct ScannerResourceDescriptor(NitroxTechType TechType, ushort TrackerIndex, NitroxVector3 RelativePosition);
