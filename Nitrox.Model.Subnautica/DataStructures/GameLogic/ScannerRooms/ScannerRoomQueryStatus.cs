namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

public enum ScannerRoomQueryStatus : byte
{
    Complete,
    NotModified,
    InvalidRoom,
    OriginUnavailable,
    Rejected,
    Failed
}
