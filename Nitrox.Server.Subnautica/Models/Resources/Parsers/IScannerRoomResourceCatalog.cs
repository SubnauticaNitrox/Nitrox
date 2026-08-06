using System.Collections.Generic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal interface IScannerRoomResourceCatalog
{
    float MaximumRelativeOffset { get; }

    bool IsKnownTechType(NitroxTechType techType);

    bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors);
}
