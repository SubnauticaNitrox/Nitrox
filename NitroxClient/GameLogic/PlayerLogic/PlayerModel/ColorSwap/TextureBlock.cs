namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap
{
    public class TextureBlock
    {
        public int X { get; }
        public int Y { get; }
        public int BlockWidth { get; }
        public int BlockHeight { get; }

        public TextureBlock(int x, int y, int blockWidth, int blockHeight)
        {
            X = x;
            Y = y;
            BlockWidth = blockWidth;
            BlockHeight = blockHeight;
        }
    }
}
