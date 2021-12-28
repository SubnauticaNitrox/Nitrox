using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic.Buildings.Metadata
{
    [ZeroFormattable]
    [ProtoContract]
    public class SignMetadata : BasePieceMetadata
    {
        [Index(0)]
        [ProtoMember(1)]
        public virtual string Text { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual int ColorIndex { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual int ScaleIndex { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual bool[] Elements { get; set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual bool Background { get; set; }

        public SignMetadata()
        {
            //Constructor for serialization. Has to be "protected" for json serialization.
        }

        public SignMetadata(string text, int colorIndex, int scaleIndex, bool[] elements, bool backgroundToggle)
        {
            Text = text;
            ColorIndex = colorIndex;
            ScaleIndex = scaleIndex;
            Elements = elements;
            Background = backgroundToggle;
        }

        public override string ToString()
        {
            return "[SignMetadata - Text: " + Text + " ColorIndex: " + ColorIndex + "ScaleIndex: " + ScaleIndex + " Elements: " + Elements + " Background: " + Background + "]";
        }
    }
}
