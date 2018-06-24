using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SignChanged : Packet
    {
        public string Guid { get; }
        public string NewText { get; }
        public int ColorIndex { get; }
        public int ScaleIndex { get; }
        public bool[] Elements { get; }
        public bool Background { get; }

        public SignChanged(string guid, string newText, int colorIndex, int scaleIndex, bool[] elements, bool backgroundToggle)
        {
            Guid = guid;
            NewText = newText;
            ColorIndex = colorIndex;
            ScaleIndex = scaleIndex;
            Elements = elements;
            Background = backgroundToggle;
        }
    }
}
