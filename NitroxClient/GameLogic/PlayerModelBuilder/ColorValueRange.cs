namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    public class ColorValueRange
    {
        protected float min { get; set; }
        protected float max { get; set; }

        public ColorValueRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Covers(float targetValue)
        {
            return min <= targetValue && targetValue <= max;
        }
    }
}
