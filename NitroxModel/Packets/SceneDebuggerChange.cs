using ZeroFormatter;

namespace NitroxModel.Packets
{
    [ZeroFormattable]
    public class SceneDebuggerChange : Packet
    {
        [Index(0)]
        public virtual string Path { get; protected set; }
        [Index(1)]
        public virtual int GameObjectID { get; protected set; }
        [Index(2)]
        public virtual int ComponentID { get; protected set; }
        [Index(3)]
        public virtual string FieldName { get; protected set; }
        [Index(4)]
        public virtual string Value { get; protected set; }

        private SceneDebuggerChange() { }

        public SceneDebuggerChange(string path, int gameObjectID, int componentID, string fieldName, string value)
        {
            Path = path;
            GameObjectID = gameObjectID;
            ComponentID = componentID;
            FieldName = fieldName;
            Value = value;
        }
    }
}
