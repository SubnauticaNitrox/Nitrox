using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Messaging;

namespace Nitrox.Launcher.Models.Extensions;

public static class MessageReceiverExtensions
{
    public static void RegisterMessageListener<T, TReceiver>(this TReceiver receiver, Func<T, TReceiver, Task> asyncFunc) where T : class where TReceiver : IMessageReceiver
    {
        if (WeakReferenceMessenger.Default.IsRegistered<T>(receiver))
        {
            WeakReferenceMessenger.Default.Unregister<T>(receiver);
        }
        WeakReferenceMessenger.Default.Register<T>(receiver, (_, message) => asyncFunc(message, receiver));
    }

    public static void RegisterMessageListener<T, TReceiver>(this TReceiver receiver, Action<T, TReceiver> action) where T : class where TReceiver : IMessageReceiver
    {
        if (WeakReferenceMessenger.Default.IsRegistered<T>(receiver))
        {
            WeakReferenceMessenger.Default.Unregister<T>(receiver);
        }
        WeakReferenceMessenger.Default.Register<T>(receiver, (_, message) => action(message, receiver));
    }
}
