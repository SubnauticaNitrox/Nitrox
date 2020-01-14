namespace NitroxServer.Serialization.Resources.Datastructures
{
    public class PrefabAsset
    {
        public string ClassId { get; set; }
        public TransformAsset TransformAsset { get; set; }

        public PrefabAsset(string classId, TransformAsset transformAsset)
        {
            ClassId = classId;
            TransformAsset = transformAsset;
        }
    }
}
