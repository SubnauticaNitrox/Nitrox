using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class KeypadMetadataExtractor : EntityMetadataExtractor<KeypadDoorConsole, KeypadMetadata>
{
    public override KeypadMetadata Extract(KeypadDoorConsole keypadDoorConsole)
    {
        string pathToRoot = string.Empty;
        
        if (keypadDoorConsole.root)
        {
            pathToRoot = keypadDoorConsole.gameObject.GetHierarchyPath(keypadDoorConsole.root);
        }
        
        return new(keypadDoorConsole.unlocked, pathToRoot);
    }
}
