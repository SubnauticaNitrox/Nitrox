using ProtoBuf;
using System;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class SignData
    {
        [ProtoMember(1)]
        public string Guid { get; }
        [ProtoMember(2)]
        public string NewText { get; set; }
        [ProtoMember(3)]
        public int ColorIndex { get; set; }
        [ProtoMember(4)]
        public int ScaleIndex { get; set; }
        [ProtoMember(5)]
        public bool[] Elements { get; set; }
        [ProtoMember(6)]
        public bool Background { get; set; }

        public SignData()
        {
            //Constructor Serializacion
        }

        public SignData(string guid, string newText, int colorIndex, int scaleIndex, bool[] elements, bool backgroundToggle)
        {
            Guid = guid;
            NewText = newText;
            ColorIndex = colorIndex;
            ScaleIndex = scaleIndex;
            Elements = elements;
            Background = backgroundToggle;
        }

        public override string ToString()
        {
            return "[SignContainer - Container: Guid: " + Guid + " NewText: " + NewText + " ColorIndex: " + ColorIndex + "ScaleIndex: " + ScaleIndex + " Elements: " + Elements + " Background: " + Background + "]";
        }
    }
}
