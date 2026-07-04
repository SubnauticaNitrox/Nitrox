namespace NitroxClient.GameLogic.Simulation;

public sealed class MapRoomCameraControl(MapRoomCamera camera, MapRoomScreen screen, string reason, bool showStatus) : LockRequestContext
{
    public MapRoomCamera Camera { get; } = camera;
    public MapRoomScreen Screen { get; } = screen;
    public string Reason { get; } = reason;
    public bool ShowStatus { get; } = showStatus;
}
