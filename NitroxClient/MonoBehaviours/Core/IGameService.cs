namespace NitroxClient.MonoBehaviours.Core;

/// <summary>
///     Provides callbacks to run code on the main game thread.
/// </summary>
internal interface IGameService
{
    /// <summary>
    ///     Called for every game update tick. Executes on the main game thread.
    /// </summary>
    void Update();

    void SceneChange(string name);
}
