using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata
{
    public class IncubatorMetadataProcessor : GenericEntityMetadataProcessor<IncubatorMetadata>
    {
        public override void ProcessMetadata(GameObject gameObject, IncubatorMetadata metadata)
        {
            Incubator incubator = gameObject.GetComponent<Incubator>();
            incubator.powered = metadata.Powered;
            incubator.hatched = metadata.Hatched;

            bool terminalActive = (bool)incubator.ReflectionGet("terminalActive");
            incubator.computerTerminal.SetActive(terminalActive);
            incubator.ReflectionSet("emissiveIntensity", (float)(terminalActive ? 1 : 0));
            incubator.ReflectionCall("UpdateMaterialsEmissive");
            incubator.terminalLight.intensity = (float)incubator.ReflectionGet("emissiveIntensity") * 1.85f;
        }
    }
}
