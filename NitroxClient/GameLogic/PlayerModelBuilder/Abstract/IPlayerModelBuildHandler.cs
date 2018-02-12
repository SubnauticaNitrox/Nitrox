namespace NitroxClient.GameLogic.PlayerModelBuilder.Abstract
{
    public interface IPlayerModelBuildHandler : IPlayerModelBuilder
    {
        IPlayerModelBuildHandler Successor { get; }
    }
}
