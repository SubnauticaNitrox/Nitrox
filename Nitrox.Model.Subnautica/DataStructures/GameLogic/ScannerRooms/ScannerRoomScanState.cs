using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

/// <summary>
/// The server-owned resource selection for one Scanner Room.
/// </summary>
[Serializable, DataContract]
public sealed class ScannerRoomScanState
{
    public static ScannerRoomScanState Empty { get; } = new(null, 0);

    [DataMember(Order = 1, EmitDefaultValue = false)]
    public NitroxTechType? SelectedTechType { get; }

    [DataMember(Order = 2)]
    public ulong Version { get; }

    [IgnoreConstructor]
    protected ScannerRoomScanState()
    {
        // Constructor for serialization. Has to be protected for JSON serialization.
    }

    public ScannerRoomScanState(NitroxTechType? selectedTechType, ulong version)
    {
        SelectedTechType = selectedTechType?.Equals(NitroxTechType.None) == true ? null : selectedTechType;
        Version = version;
    }

    public override string ToString() => $"[SelectedTechType: {SelectedTechType}, Version: {Version}]";
}
