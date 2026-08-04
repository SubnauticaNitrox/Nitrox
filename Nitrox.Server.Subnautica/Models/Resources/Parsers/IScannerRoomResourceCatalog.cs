using System.Collections.Generic;

namespace Nitrox.Server.Subnautica.Models.Resources.Parsers;

internal interface IScannerRoomResourceCatalog
{
    float MaximumRelativeOffset { get; }

    bool TryGetDescriptors(string classId, out IReadOnlyList<ScannerResourceDescriptor> descriptors);
}
