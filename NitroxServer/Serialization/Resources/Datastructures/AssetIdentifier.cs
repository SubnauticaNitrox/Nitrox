namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class AssetIdentifier
    {
        public uint FileId { get; }
        public ulong IndexId { get; }

        public AssetIdentifier(uint fileId, ulong indexId)
        {
            FileId = fileId;
            IndexId = indexId;
        }

        public override bool Equals(object obj)
        {
            AssetIdentifier identifier = obj as AssetIdentifier;

            return identifier != null &&
                   FileId == identifier.FileId &&
                   IndexId == identifier.IndexId;
        }

        public override int GetHashCode()
        {
            int hashCode = 390124324;

            hashCode = hashCode * -1521134295 + FileId.GetHashCode();
            hashCode = hashCode * -1521134295 + IndexId.GetHashCode();

            return hashCode;
        }
    }
}
