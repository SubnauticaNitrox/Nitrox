using System;
using System.Threading.Channels;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Core;

/// <summary>
///     Pushes game updates to all Nitrox services so they have a chance to interact with the game on the main thread.
/// </summary>
internal sealed class NitroxServicesManager : MonoBehaviour
{
    private static readonly Channel<Action> unityThreadTasks = Channel.CreateUnbounded<Action>();
    public IGameService[] GameServices = [];
    public IMultiplayerGameService[] MultiplayerServices = [];

    /// <summary>
    ///     Adds a task for Unity to execute on the main thread.
    /// </summary>
    public static async Task AddUnityTaskAsync(Action action) => await unityThreadTasks.Writer.WriteAsync(action);

    /// <summary>
    ///     Called every game update tick. Executes on the main game thread.
    /// </summary>
    private void Update()
    {
        while (unityThreadTasks.Reader.TryRead(out Action task))
        {
            task();
        }
        foreach (IGameService service in GameServices)
        {
            service.Update();
        }
        if (Multiplayer.Active)
        {
            foreach (IMultiplayerGameService service in MultiplayerServices)
            {
                service.Update();
            }
        }
    }

    /// <summary>
    ///     Sets the processing priority of a type. Higher priority means it gets called earlier in the stack.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class PriorityAttribute(uint priority) : Attribute
    {
        public uint Priority { get; } = priority;
    }
}
