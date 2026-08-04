using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Extensions;
using NitroxClient.Extensions;
using NitroxClient.GameLogic.ScannerRooms;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Binds a spawned Scanner Room to its multiplayer identity and translates vanilla Scanner Room interactions into
/// authoritative snapshot requests.
/// </summary>
[DisallowMultipleComponent]
public sealed class ScannerRoomController : MonoBehaviour
{
    private MapRoomFunctionality mapRoom = null!;
    private ScannerRoomRequestTrigger? requestTrigger;

    public static ScannerRoomController Attach(MapRoomFunctionality mapRoom, NitroxId mapRoomId)
    {
        if (!mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            controller = mapRoom.gameObject.AddComponent<ScannerRoomController>();
        }

        controller.Initialize(mapRoom, mapRoomId);
        return controller;
    }

    public void RequestInitialSnapshot()
    {
        if (requestTrigger == null || !TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin))
        {
            return;
        }

        requestTrigger.TryRequestInitial(range, selectedTechType, observedOrigin);
    }

    public void RequestImmediateSnapshot()
    {
        if (requestTrigger == null || !TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin))
        {
            return;
        }

        requestTrigger.RequestImmediate(range, selectedTechType, observedOrigin);
    }

    private void Initialize(MapRoomFunctionality mapRoom, NitroxId mapRoomId)
    {
        if (requestTrigger != null)
        {
            return;
        }

        this.mapRoom = mapRoom;
        ScannerRoomManager scannerRoomManager = this.Resolve<ScannerRoomManager>();
        requestTrigger = new ScannerRoomRequestTrigger(
            (range, selectedTechType, observedOrigin) => scannerRoomManager.RequestSnapshot(mapRoomId, range, selectedTechType, observedOrigin));
    }

    private void Start() => RequestInitialSnapshot();

    private bool TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin)
    {
        range = default;
        selectedTechType = null;
        observedOrigin = null;

        if (!mapRoom)
        {
            return false;
        }

        range = mapRoom.GetScanRange();
        selectedTechType = mapRoom.typeToScan == TechType.None ? null : mapRoom.typeToScan.ToDto();
        observedOrigin = mapRoom.wireFrameWorld ? mapRoom.wireFrameWorld.position.ToDto() : null;
        return true;
    }
}
