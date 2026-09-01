namespace NitroxClient.MonoBehaviours.Core;

/// <summary>
///     Game service which only updates while multiplayer is active.
/// </summary>
/// <remarks>
/// The order of calls is as follows:<br />
///  - <see cref="Start"/>: Called once per join.<br />
///  - <see cref="Started"/>: Called once per join, just after multiplayer finished loading.<br />
///  - <see cref="Update"/>: Called every update after <see cref="Started"/> has been called.<br />
///  - <see cref="Stop"/>: Called once per local client disconnect from server.
/// </remarks>
internal interface IMultiplayerGameService
{
    /// <summary>
    ///     Called on each game update while the client is playing in a multiplayer session.
    /// </summary>
    void Update();

    /// <summary>
    ///     Called when a multiplayer session has been locked in and about to start.
    /// </summary>
    void Start();

    /// <summary>
    ///     Called when the game finished loading all the multiplayer world data from the server.
    /// </summary>
    void Started();

    /// <summary>
    ///     Called when the client drops out of a multiplayer session.
    /// </summary>
    void Stop();
}
