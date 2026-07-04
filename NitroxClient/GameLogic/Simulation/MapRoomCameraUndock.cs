namespace NitroxClient.GameLogic.Simulation;

public sealed class MapRoomCameraUndock(MapRoomCamera camera) : LockRequestContext
{
    public MapRoomCamera Camera { get; } = camera;
}
