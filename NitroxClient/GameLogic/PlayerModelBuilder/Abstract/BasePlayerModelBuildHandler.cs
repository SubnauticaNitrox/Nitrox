namespace NitroxClient.GameLogic.PlayerModelBuilder.Abstract
{
    //See: Chain of Responsibility
    public abstract class BasePlayerModelBuildHandler : IPlayerModelBuildHandler
    {
        public void Build(RemotePlayer player)
        {
            HandleBuild(player);
            Successor?.Build(player);
        }

        public IPlayerModelBuildHandler SetSuccessor(IPlayerModelBuildHandler successor)
        {
            Successor = successor;
            return Successor;
        }

        protected abstract void HandleBuild(RemotePlayer player);
        public IPlayerModelBuildHandler Successor { get; private set; }
    }
}
