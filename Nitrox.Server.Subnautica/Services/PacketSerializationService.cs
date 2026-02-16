using System.IO;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class PacketSerializationService : BackgroundService
{
    private readonly TaskCompletionSource init;
    private readonly ILogger<PacketSerializationService> logger;
    private IPacketSerializer inner;
    private readonly Lock innerLock = new();

    public PacketSerializationService(ILogger<PacketSerializationService> logger)
    {
        this.logger = logger;

        init = new TaskCompletionSource();
        inner = new UnloadedSerializer(init, serializer =>
        {
            lock (innerLock)
            {
                inner = serializer;
            }
        });
    }

    public void SerializeInto(Packet packet, Stream stream)
    {
        IPacketSerializer actual;
        lock (innerLock)
        {
            actual = inner;
        }
        actual.SerializeInto(packet, stream);
    }

    public override void Dispose()
    {
        if (init.Task.IsCompleted)
        {
            init.Task.Dispose();
        }
        base.Dispose();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Run(() =>
        {
            try
            {
                Packet.InitSerializer();
            }
            catch (Exception ex)
            {
                init.TrySetException(ex);
            }
            if (!init.TrySetResult())
            {
                throw new Exception("Failed to set init result");
            }
        }, stoppingToken).ContinueWithHandleError(exception => logger.ZLogCritical(exception, $"Failed to initialize packet serializer"));
    }

    private sealed class UnloadedSerializer(TaskCompletionSource init, Action<IPacketSerializer> implementationSwapper) : IPacketSerializer
    {
        public void SerializeInto(Packet packet, Stream stream)
        {
            if (!init.Task.IsCompletedSuccessfully)
            {
                init.Task.Wait(TimeSpan.FromSeconds(10));
            }
            packet.SerializeInto(stream);
            implementationSwapper(new LoadedSerializer());
        }
    }

    private sealed class LoadedSerializer : IPacketSerializer
    {
        public void SerializeInto(Packet packet, Stream stream) => packet.SerializeInto(stream);
    }

    private interface IPacketSerializer
    {
        void SerializeInto(Packet packet, Stream stream);
    }
}
