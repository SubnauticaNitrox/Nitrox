using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PictureFrameMetadataExtractor : EntityMetadataExtractor<PictureFrame, PictureFrameMetadata>
{
    public override PictureFrameMetadata Extract(PictureFrame pictureFrame)
    {
        return new(string.IsNullOrEmpty(pictureFrame.fileName) ? null : pictureFrame.fileName);
    }
}
