namespace NitroxClient.GameLogic.Simulation
{
    public class HandInteraction<T> : LockRequestContext where T : IHandTarget
    {
        public T Target { get; }
        public GUIHand GuiHand { get; }

        public HandInteraction(T Target, GUIHand GuiHand)
        {
            this.Target = Target;
            this.GuiHand = GuiHand;
        }
    }
}
