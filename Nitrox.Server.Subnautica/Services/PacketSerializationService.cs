using System.IO;

namespace Nitrox.Server.Subnautica.Services;

internal sealed class PacketSerializationService : BackgroundService
{
    private readonly TaskCompletionSource initTcs;
    private readonly ILogger<PacketSerializationService> logger;
    private IPacketSerializer inner;

    public PacketSerializationService(ILogger<PacketSerializationService> logger)
    {
        this.logger = logger;

        initTcs = new TaskCompletionSource();
        inner = new UnloadedSerializer(initTcs, serializer => Interlocked.Exchange(ref inner, serializer));
    }

    public void SerializeInto(Packet packet, Stream stream)
    {
        IPacketSerializer serializer = Interlocked.CompareExchange(ref inner, null, null);
        serializer.SerializeInto(packet, stream);
    }

    public override void Dispose()
    {
        if (initTcs.Task.IsCompleted)
        {
            initTcs.Task.Dispose();
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
                initTcs.TrySetException(ex);
            }
            if (!initTcs.TrySetResult())
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
