using System;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

/// <summary>
///     Represents a piece of a player's base. E.g. a window, entry/exit hatch, multi-purpose room.
///     The <see cref="TechType"/> says what kind of base piece this is.
/// </summary>
[Serializable]
[JsonContractTransition]
public class BasePiece
{
    [JsonMemberTransition]
    public NitroxId Id { get; set; }

    [JsonMemberTransition]
    public NitroxVector3 ItemPosition { get; set; }

    [JsonMemberTransition]
    public NitroxQuaternion Rotation { get; set; }

    [JsonMemberTransition]
    public NitroxTechType TechType { get; set; }

    [JsonMemberTransition]
    public Optional<NitroxId> ParentId { get; set; }

    [JsonMemberTransition]
    public NitroxVector3 CameraPosition { get; set; }

    [JsonMemberTransition]
    public NitroxQuaternion CameraRotation { get; set; }

    [JsonMemberTransition]
    public float ConstructionAmount { get; set; }

    [JsonMemberTransition]
    public bool ConstructionCompleted { get; set; }

    [JsonMemberTransition]
    public bool IsFurniture { get; set; }

    [JsonMemberTransition]
    public NitroxId BaseId { get; set; }

    [JsonMemberTransition]
    public Optional<BuilderMetadata> RotationMetadata { get; set; }

    [JsonMemberTransition]
    public Optional<BasePieceMetadata> Metadata { get; set; }

    [JsonMemberTransition]
    public int BuildIndex { get; set; }

    // Constructor for serialization.
    private BasePiece() { }

    public BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, 
                     Optional<NitroxId> parentId, bool isFurniture, Optional<BuilderMetadata> rotationMetadata)
    {
        Id = id;
        ItemPosition = itemPosition;
        Rotation = rotation;
        TechType = techType;
        CameraPosition = cameraPosition;
        CameraRotation = cameraRotation;
        ParentId = parentId;
        IsFurniture = isFurniture;
        ConstructionAmount = 0.0f;
        ConstructionCompleted = false;
        RotationMetadata = rotationMetadata;
        Metadata = Optional.Empty;
    }

    internal BasePiece(NitroxId id, NitroxVector3 itemPosition, NitroxQuaternion rotation, NitroxVector3 cameraPosition, NitroxQuaternion cameraRotation, NitroxTechType techType, 
                       Optional<NitroxId> parentId, bool isFurniture, float constructionAmount, bool constructionCompleted, int buildIndex, Optional<BuilderMetadata> rotationMetadata, Optional<BasePieceMetadata> metadata) : 
        this(id, itemPosition, rotation, cameraPosition, cameraRotation, techType, parentId, isFurniture, rotationMetadata)
    {
        ConstructionAmount = constructionAmount;
        ConstructionCompleted = constructionCompleted;
        Metadata = metadata;
        BuildIndex = buildIndex;
    }

    public override string ToString()
    {
        return $"[BasePiece - Id: {Id}, ItemPosition: {ItemPosition}, Rotation: {Rotation}, TechType: {TechType}, ParentId: {ParentId}, CameraPosition: {CameraPosition}, CameraRotation: {CameraRotation}, ConstructionAmount: {ConstructionAmount}, ConstructionCompleted: {ConstructionCompleted}, IsFurniture: {IsFurniture}, BaseId: {BaseId}, RotationMetadata: {RotationMetadata}, Metadata: {Metadata}, BuildIndex: {BuildIndex}]";
    }
}
