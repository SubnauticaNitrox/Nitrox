using System;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases
{
    public class GeometryLayoutChangeHandler
    {
        // Signal if the baseGhost is finishing its GeometryUpdate. As mentioned above, we are only interested in GeometryUpdates of the visible
        // objects. This happens in the finishing of the baseGhost. 
        internal bool baseGhostIsFinishing = false;

        // Cache for saving and reassigning Ids on Base or Layout changes
        private List<GeometryInfo> idCacheForGeometryUpdate = new List<GeometryInfo>();


        // After constructing a new object we assign a new Id to the constructing gameobject. (Or set the transmitted id from the remote event.)
        // When a gameobject triggers a GeometryChange we need to save the Id before.
        private NitroxId preservedIdforConstructableBaseConstructing = null;

        // When a gameObject is fully constructed and is to start deconstructing we also need to transfer the Id.
        private NitroxId preservedIdforConstructableBaseDeconstructing = null;

        // Some objects like Ladders or Waterparks spawn surfaces on two floors. For this we need to know which surfaces are related.
        private Dictionary<NitroxId, NitroxId> relationTopBottomIds = new Dictionary<NitroxId, NitroxId>();


        // Patches
        public void BaseGhost_Finish_Pre(BaseGhost baseGhost)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("BaseGhost_Finish_Pre");
#endif

            baseGhostIsFinishing = true;
        }

        public void Base_ClearGeometry_Pre(Base @base)
        {
            PreserveAssignedNitroxIDs(@base);
        }

        public void BaseDeconstructable_MakeCellDeconstructable_Post(GameObject gameObject)
        {
            ReassignPreservedNitroxIdsAndNewIds(gameObject.GetComponentInParent<Base>(), gameObject.transform);
        }

        public void BaseDeconstructable_MakeFaceDeconstructable_Post(GameObject gameObject)
        {
            ReassignPreservedNitroxIdsAndNewIds(gameObject.GetComponentInParent<Base>(), gameObject.transform);
        }

        public void Base_SpawnPiece_Post(Base @base, Transform surface)
        {
            ReassignPreservedNitroxIdsAndNewIds(@base, surface);
        }

        public void BaseGhost_Finish_Post(BaseGhost instance)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("BaseGhost_Finish_Post");
#endif

            baseGhostIsFinishing = false;
        }

        public void BaseDeconstructable_Deconstruct_Pre(BaseDeconstructable instance)
        {
            PreserveIdFromFinishedObjectThatBeginsDeconstruction(instance);
        }

        public void ConstructableBase_SetState_Pre(ConstructableBase instance, bool value, bool setAmount)
        {
            // Cache the id of a constructableBase that gets constructed
            if (value == true && setAmount == true && !instance._constructed)
            {
                PreserveIdFromNewObjectThatGetsPartlyConstructedOrFinished(instance.gameObject);
            }

            // Reassign the cached id from a start of deconstruction to the now constructable object
            if (value == false && setAmount == false && instance._constructed)
            {
                ReassignPreservedNitroxIdfromBeginningDeconstruction(instance.gameObject);
            }
        }

        public void Constructable_Deconstruct_Post(Constructable instance, bool result)
        {
            if (result && instance.constructedAmount <= 0f)
            {
                NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                if (id != null)
                {
                    RemovePreservedTopBottomRelationIds(id);
                }
            }
        }


        // Logic

        internal void PreserveAssignedNitroxIDs(Base @base)
        {
            idCacheForGeometryUpdate.Clear();

            Transform[] cellObjects = (Transform[])@base.ReflectionGet("cellObjects");

            if (cellObjects == null)
            {
                return;
            }

            foreach (Transform cellObject in cellObjects)
            {
                if (cellObject != null)
                {
                    for (int i = 0; i < cellObject.childCount; i++)
                    {
                        Transform surface = cellObject.GetChild(i);

                        if (surface && surface.gameObject)
                        {

#if TRACE && BUILDING
                            string info = generateGameObjectTransformInfo(surface,0);
                            NitroxModel.Logger.Log.Debug("Base_ClearGeometry_Pre - current clear for : " + info);
#endif

                            if (!surface.name.Contains("CorridorConnector")) // Have to ignore all CorridorConnectors, because they are automatically spawned in case of adding a Corridor or a Hatch to a Room
                            {

                                NitroxId id = NitroxEntity.GetIdNullable(surface.gameObject);
                                if (id != null)
                                {

#if TRACE && BUILDING
                                    NitroxModel.Logger.Log.Debug("Base_ClearGeometry_Pre - saving id for : " + info + " id: " + id);
#endif

                                    string parentCellContent = string.Empty;
                                    if (surface.GetSiblingIndex() == 0)
                                    {
                                        for (int j = 0; j < surface.parent.childCount; j++)
                                        {
                                            if (surface.gameObject != null)
                                            {
                                                parentCellContent = parentCellContent + surface.parent.GetChild(j).name;
                                            }
                                        }
                                    }

                                    GeometryInfo geometryInfo = new GeometryInfo() { Name = surface.name, Id = id, CellPosition = surface.parent.position, CellIndex = surface.GetSiblingIndex(), CellContent = parentCellContent };
                                    idCacheForGeometryUpdate.Add(geometryInfo);
                                    NitroxEntity.RemoveId(surface.gameObject);
                                }
                                else
                                {
#if TRACE && BUILDING
                                    NitroxModel.Logger.Log.Error("Base_ClearGeometry_Pre - no NitroxId found on GameObject: " + surface.name + " cellPosition: " + surface.parent.position + " surfaceIndex: " + surface.GetSiblingIndex());
#endif
                                }
                            }
                        }
                    }
                }
            }
        }

        internal void ReassignPreservedNitroxIdsAndNewIds(Base @base, Transform surface)
        {
            if (!@base.isGhost && surface.parent.position != Vector3.zero)
            {

#if TRACE && BUILDING
                NitroxId baseId = NitroxEntity.GetIdNullable(@base.gameObject);
                NitroxId pieceId = NitroxEntity.GetIdNullable(surface.gameObject);
                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - base: " + @base + " baseId: " + baseId + " piece: " + surface.gameObject + " pieceID: " + pieceId + " piecePosition: " + surface.position + " pieceIndex: " + surface.GetSiblingIndex() + " pieceCellPosition: " + surface.parent.position + " pieceCellIndex: " + surface.parent.GetSiblingIndex());
                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - baseGhostIsFinishing: " + baseGhostIsFinishing + " transferIdforConstructableBaseConstructing: " + preservedIdforConstructableBaseConstructing + " piece: " + surface.gameObject + " pieceID: " + pieceId + " piecePosition: " + surface.position + " pieceIndex: " + surface.GetSiblingIndex() + " pieceCellPosition: " + surface.parent.position + " pieceCellIndex: " + surface.parent.GetSiblingIndex());
#endif
                if (surface.name.Contains("CorridorConnector"))
                {
                    return;
                }
#if TRACE && BUILDING
                else
                {
                    NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - no CorridorConnector, continuing");
                }
#endif


                NitroxId id = NitroxEntity.GetIdNullable(surface.gameObject);
                if (id != null)
                {
                    return;
                }
#if TRACE && BUILDING
                else
                {
                    NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - found no existing id, continuing");
                }
#endif


                if (idCacheForGeometryUpdate.Count > 0)
                {
                    if (checkForConnectorTubeReplacement(surface, out id))
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface, 0) + " id: " + id);
#endif
                        NitroxEntity.SetNewId(surface.gameObject, id);
                        return;
                    }
#if TRACE && BUILDING
                    else
                    {
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - found no ConnectorTubeReplacement");
                    }
#endif
                    int siblingIndexOffset = 0;
                    if (surface.GetSiblingIndex() != 0)
                    // Only need to do this for childs with index > 0, because the index 0 is always the cellobject itself (e.g. room, corridor)
                    {
                        siblingIndexOffset = checkForMovedFaceIndex(surface);
                    }
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - calculated siblingIndexOffset: " + siblingIndexOffset);
#endif

                    GeometryInfo info = idCacheForGeometryUpdate.Find(g => g.Name == surface.name && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex() + siblingIndexOffset);
                    if (info != null)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface, siblingIndexOffset) + " id: " + info.Id);
#endif
                        NitroxEntity.SetNewId(surface.gameObject, info.Id);
                        return;
                    }

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - found no cached id for : " + generateGameObjectTransformInfo(surface, siblingIndexOffset));
#endif

                }
#if TRACE && BUILDING
                else
                {
                    NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - no cached ids found");
                }
#endif


#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - object has constructableBase : " + surface.gameObject.GetComponent<ConstructableBase>());
#endif

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - object has deconstructableBase : " + surface.gameObject.GetComponent<BaseDeconstructable>());
#endif

                //only do this when we are not in ghost mode
                if (baseGhostIsFinishing && surface.gameObject.GetComponent<BaseDeconstructable>())
                {

                    // If no cached id could be found, but we have a cachedID from a new Piece
                    if (preservedIdforConstructableBaseConstructing != null)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface,0) + " id: " + preservedIdforConstructableBaseConstructing);
#endif

                        NitroxEntity.SetNewId(surface.gameObject, preservedIdforConstructableBaseConstructing);

                        if (surface.name.Contains("BaseRoomWaterParkBottom") || surface.name.Contains("BaseRoomLadderTop") || surface.name.Contains("BaseCorridorLadderTop"))
                        // For WaterParks the BottomShape is always spawned first. For Ladders the TopShape is alwayse spawned first, no matter if build from the lower or upper floor.
                        {
                            if (!relationTopBottomIds.ContainsKey(preservedIdforConstructableBaseConstructing))
                            {
                                NitroxId nitroxIdforOtherPart = new NitroxId();
                                relationTopBottomIds.Add(preservedIdforConstructableBaseConstructing, nitroxIdforOtherPart);
                                preservedIdforConstructableBaseConstructing = nitroxIdforOtherPart;

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - caching a new Id for the other Top or Bottom part - cachedPieceIdFromConstructableBasePre: " + preservedIdforConstructableBaseConstructing);
#endif

                            }
                            else
                            {
                                preservedIdforConstructableBaseConstructing = null;
                            }
                        }
                        else
                        {
                            preservedIdforConstructableBaseConstructing = null;
                        }
                    }
                }
            }
        }

        private int checkForMovedFaceIndex(Transform surface)
        {
            GeometryInfo cachedInfo = idCacheForGeometryUpdate.Find(g => g.CellPosition == surface.parent.position && g.CellIndex == 0);
            if (cachedInfo == null) //in case a new CellObject is spawned, there is no cachedInfo for this one
            {
                return 0;
            }

            string currentParentContent = string.Empty;
            for (int i = 0; i < surface.parent.childCount; i++)
            {
                currentParentContent = currentParentContent + surface.parent.GetChild(i).name;
            }

            // Check if the FaceIndex has moved. Multiple changes can happen at the same time. 
            int moveFaceIndex = 0;

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - cached ParentContent : " + cachedInfo.CellContent);
            NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - current ParentContent : " + currentParentContent);
#endif

            foreach (object item in Enum.GetValues(typeof(RemoveableSurface)))
            {
                if (cachedInfo.CellContent.Contains(item.ToString()) && !currentParentContent.Contains(item.ToString()))
                // a removable surface has been removed
                {
                    moveFaceIndex = moveFaceIndex + 1;
                }
                if (currentParentContent.Contains(item.ToString()) && !cachedInfo.CellContent.Contains(item.ToString()))
                // a formerly removed surface has been readded
                {
                    moveFaceIndex = moveFaceIndex - 1;
                }
            }

            return moveFaceIndex;

        }

        private bool checkForConnectorTubeReplacement(Transform surface, out NitroxId id)
        {
            // In case a vertical connector exists and a ladder is build inside the base, then this vertical connector changes its shape and name
            if (!surface.name.Contains("ConnectorTube"))
            {
                id = null;
                return false;
            }

            if (surface.name.Contains("Window"))
            {
                GeometryInfo found = idCacheForGeometryUpdate.Find(g => g.Name == surface.name.Replace("ConnectorTubeWindow", "ConnectorTube") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex());
                if (found != null)
                {
                    id = found.Id;
                    return true;
                }
            }
            else
            {
                GeometryInfo found = idCacheForGeometryUpdate.Find(g => g.Name == surface.name.Replace("ConnectorTube", "ConnectorTubeWindow") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex());
                if (found != null)
                {
                    id = found.Id;
                    return true;
                }
            }

            id = null;
            return false;
        }

        internal void ReassignPreservedNitroxIdfromBeginningDeconstruction(GameObject gameObject)
        {
            if (preservedIdforConstructableBaseDeconstructing != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("ConstructableBase_SetState_Pre - setting id from transferIdforConstructableBaseDeconstructing: " + preservedIdforConstructableBaseDeconstructing);
#endif

                NitroxEntity.SetNewId(gameObject, preservedIdforConstructableBaseDeconstructing);
                preservedIdforConstructableBaseDeconstructing = null;
            }
        }

        internal void PreserveIdFromNewObjectThatGetsPartlyConstructedOrFinished(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetIdNullable(gameObject);
            if (id != null)
            {
                preservedIdforConstructableBaseConstructing = id;
                NitroxEntity.RemoveId(gameObject);

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("ConstructableBase_SetState_Pre - caching id to transferIdforConstructableBaseConstructing: " + preservedIdforConstructableBaseConstructing);
#endif
            }
        }

        internal void PreserveIdFromFinishedObjectThatBeginsDeconstruction(BaseDeconstructable instance)
        {
            NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
            if (id == null)
            {
                NitroxModel.Logger.Log.Error("BaseDeconstructable_Deconstruct_Pre - Trying to deconstruct an Object that has no NitroxId - gameObject: " + instance.gameObject);
            }
            else
            {
                if (instance.name.Contains("BaseRoomWaterParkTop") || instance.name.Contains("BaseRoomLadderBottom") || instance.name.Contains("BaseCorridorLadderBottom"))
                {
                    foreach (KeyValuePair<NitroxId, NitroxId> item in relationTopBottomIds)
                    {
                        if (item.Value == id)
                        {
                            preservedIdforConstructableBaseDeconstructing = item.Key;
                            NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(item.Key).Value);
                            NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(item.Value).Value);
                            break;
                        }
                    }
                }
                else
                {
                    preservedIdforConstructableBaseDeconstructing = id;
                    NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(id).Value);
                }

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseDeconstructable_Deconstruct_Pre - saving guid from instance: " + instance.name + " cachedPieceIdFromConstructableBaseFinished: " + preservedIdforConstructableBaseDeconstructing);
#endif
            }
        }

        internal void ClearPreservedIdForConstructing()
        {
            preservedIdforConstructableBaseConstructing = null;
        }

        internal void RemovePreservedTopBottomRelationIds(NitroxId id)
        {
            if (relationTopBottomIds.ContainsKey(id))
            {
                if (NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).HasValue)
                {
                    NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).Value);
                }
                relationTopBottomIds.Remove(id);
            }
        }

        private class GeometryInfo
        {
            internal string Name;
            internal NitroxId Id;
            internal Vector3 CellPosition;
            internal int CellIndex;
            internal string CellContent;
        }

        private enum RemoveableSurface
        {
            RoomExteriorTop,
            RoomExteriorBottom,
            AdjustableSupport
        }


#if TRACE && BUILDING
        private static string generateGameObjectTransformInfo(Transform transform, int offset)
        {
            return transform.name + " " + transform.parent.position.ToString() + " " + (transform.GetSiblingIndex() +  offset).ToString();
        }
#endif
    }
}
