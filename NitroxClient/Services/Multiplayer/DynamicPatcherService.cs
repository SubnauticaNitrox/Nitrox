using NitroxClient.MonoBehaviours.Core;

namespace NitroxClient.Services.Multiplayer;

[NitroxServicesManager.Priority(uint.MaxValue)]
internal sealed class DynamicPatcherService(PatcherService patcherService) : IMultiplayerGameService
{
    private readonly PatcherService patcherService = patcherService;

    public void Update()
    {
    }

    public void Start() => patcherService.Apply();

    public void Started()
    {
    }

    public void Stop() => patcherService.Restore();
}
