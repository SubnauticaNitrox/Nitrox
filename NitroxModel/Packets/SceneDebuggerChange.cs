using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SceneDebuggerChange : Packet
    {
        public string Path { get; }
        public int GameObjectID { get; }
        public int ComponentID { get; }
        public string FieldName { get; }
        public object Value { get; }

        public SceneDebuggerChange(string path, int gameObjectID, int componentID, string fieldName, object value)
        {
            Path = path;
            GameObjectID = gameObjectID;
            ComponentID = componentID;
            FieldName = fieldName;
            Value = value;
        }

        public override string ToString()
        {
            return "[SceneDebuggerChange Path: " + Path + " GameObjectID: " + GameObjectID + " ComponentID: " + ComponentID + " FieldName: " + FieldName + " Value: " + Value + "]";
        }
    }
}
