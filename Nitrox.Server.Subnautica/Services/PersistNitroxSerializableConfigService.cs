using System.IO;
using Nitrox.Model.Serialization;
using Nitrox.Server.Subnautica.Models.AppEvents;

namespace Nitrox.Server.Subnautica.Services;

/// <summary>
///     Saves <see cref="NitroxConfig" /> to their <see cref="SerializableFileNameAttribute" /> file in the same save
///     directory of the active server.
/// </summary>
internal sealed class PersistNitroxSerializableConfigService(IOptions<SubnauticaServerOptions> options) : IHostedService, ISaveState
{
    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public async Task OnEventAsync(ISaveState.Args args)
    {
        await Task.Run(() =>
        {
            string targetFilePath = Path.Combine(args.SavePath, SerializableFileNameAttribute.GetFileName<SubnauticaServerOptions>());
            NitroxConfig.CreateFile(targetFilePath, options.Value);
        });
    }
}
