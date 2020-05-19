using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Building
    {
        /* General Info on this class and specially about handling NitroxIds and Events
         * 
         * For understanding the logic of this class, it is first needed to understand the basics of buildable objects 
         * and object positioning in Subnautica. Buildable objects are divided into:
         * 
         * Bases
         * - Bases are all related Base-Piece objects of one Base-Complex. This is represented in Subnautica by a virtual
         *   Base-Object that is generated with the first Base-Hull object and assigned to all related Base-Pieces
         *   
         * Base-Hull objects (e.g. Corridors, Rooms, Moonpool, ..)
         * - These objects define the fundamental layout of a Base and use a cell to be placed in the world. A cell is 
         *   a defined rectangular virutal place in the world grid. A cell always only contains one Base-Hull object. 
         *   Placing the first Base-Hull object in a free space will assign a Base Object to it. Every nearby new Base-Hull
         *   object will be assigned to the Base-Object, even if not physical connected.
         * - Every Base-Hull object has more or fewer surfaces. Some of these surfaces can be replaced by Base-Integrated
         *   objects or be replaced by other surface-types according to objects in adjacent cells. All Surfaces are childs
         *   of the cell that the Base-Hull object represents. 
         * - Rely on ConstructableBase and DeconstructableBase (more on this later)
         *   
         * Base-Integrated objects (e.g. Hatches, Ladders, Reinforcements, Windows, ..)
         * - These objects replace a surface of a Base-Hull object and give it a new appearance. In default they represent 
         *   a single surface in the parent cell and have no further faces that can be build to. 
         *   There are two objects that are special:
         *   Ladders have two surfaces, one in the lower floor and one in the upper. 
         *   Waterparks are the only objects that create additional surfaces for hatches.
         * - These objects can be referenced by the child-index of a cell. (The index can change, see below for more on this.)
         * - Rely on ConstructableBase and DeconstructableBase (more on this later)
         * 
         * Base-Attached objects and outside placable Objects (e.g. Fabricator, Lockers, Solar, ...)
         * - These objects are placed by Position and are automaticaly snapped to the corresponding Base-Object. Some 
         *   functionality of these objects also needs a proper link to a Base (e.g. Power)
         * - These objects are not referenced via cell or faces and do not influence the Base-Layout.
         * - Rely on Constructable (more on this later)
         * 
         * Furniture (e.g. Tables, Chairs, ...)
         * - These objects are simply placed by Position and don't rely on other objects.
         * 
         * 
         * Constructing and Deconstructing Base-Hull objects and the Integrated-Objects is the most complex Part of this. 
         * Subnautica uses different viewmodels for objects in construction and the finished objects, which makes it hard
         * to track these objects and keep the reference via the NitroxIds. Additionally to this the models and the base-
         * layouts are pregenerated via ghost-objects for each object in the background, before the viewable object is 
         * updated in the world. A default lifecycle can be described as follows. (example with one hull and one surface)
         * 
         * Construct: create prefabGhost > create GhostBase (or assign to existing) > Clear and ReCalculateGhostGeometry >
         *   destroy prefabGhost > create Base > destroy BaseGhost > Clear and ReCalculateViewableGeometry > spawn HullConstructing > 
         *   (construct to 100%) > CalculateViewableGeometry > spawn HullFinished > (add surface object) > create HullGhost and 
         *   SurfaceGhost > CalculateGhostGeometry > destroy Ghosts and CalculateViewableGeometry > spawn HullFinished > spawn SurfaceConstructing
         * For deconstruction, the steps are nearly the same only with the Models reversed. The recalculationgeometry steps destroy
         * all gameobjects which needs a more complex handling and transferring of the NitroxIds, which are needed to identify the right objects
         * for syncing.
         * 
         * Some special things that need to be considered:
         * - Never assign an Id to a prefabghost object or a baseghost. The ghosts and their gameObjects can only be destroyed and replaced 
         *   by the viewable object when there is no NitroxEntity in GetComponent<>. This will lead to not interactable ghosts in the world 
         *   or hidden remaining ghosts in the world that prohibit multiplayer placement. The same applies for the virtual GhostBases.
         * - Every assigned NitroxId must be removed. 
         * - Objects are placed by position. This position can change according to other objects in the cell or when other objects change.
         * - The index of an object in a cell can also change, when surfaces ar attached or removed to create a new look for the object. 
         * - Keep in memory that abandoned Bases in the world also use the same mechanics for spawning and should not been interfered with
         *   mechanics here (e.g. using UnityEngine.Destroy without caution).
         */

        /*Reminder: ##TODO BUILDING## 
         * - suppress hull calculation during initialsync
         * - suppress item consumation/granting during remote events
         * - block simultanious constructing/deconstructing of same object by local and remote player
         * - sync bulkhead door state
         * minor:
         * - sync hull integrity
         * 
         */


        private readonly IPacketSender packetSender;
        private readonly RotationMetadataFactory rotationMetadataFactory;

        // Contains the last hovered constructable object hovered by the current player. Is needed to ensure fired events for the correct item.
        private Constructable lastHoveredConstructable = null;

        // State of currently using BuilderTool or not
        private bool currentlyHandlingBuilderTool = false;

        // State if a construction Event is raised by Initialsync or a current remote player.
        private bool remoteEventActive = false;

        // Signal if the baseGhost is finishing its GeometryUpdate. As mentioned above, we are only interested in GeometryUpdates of the visible
        // objects. This happens in the finishing of the baseGhost. 
        bool baseGhostIsFinished = false;

        // Cache for saving and reassigning Ids on Base or Layout changes
        private List<GeometryInfo> idCacheForGeometryUpdate = new List<GeometryInfo>();

        // After constructing a new object we assign a new Id to the constructing gameobject. (Or set the transmitted id from the remote event.)
        // When a gameobject triggers a GeometryChange we need to save the Id before.
        private NitroxId transferIdforConstructableBaseConstructing = null;

        // When a gameObject is fully constructed and is to start deconstructing we also need to transfer the Id.
        private NitroxId transferIdforConstructableBaseDeconstructing = null;

        // Some objects like Ladders or Waterparks spawn surfaces on two floors. For this we need to know which surfaces are related.
        private Dictionary<NitroxId, NitroxId> relationTopBottomIds = new Dictionary<NitroxId, NitroxId>();

        // For the base objects themself as master objects of a base-complex we can't assign Ids to the ghosts, 
        // 
        private Dictionary<GameObject, NitroxId> baseGhostsIDCache = new Dictionary<GameObject, NitroxId>();


        public Building(IPacketSender packetSender, RotationMetadataFactory rotationMetadataFactory)
        {
            this.packetSender = packetSender;
            this.rotationMetadataFactory = rotationMetadataFactory;
        }


        // Section: Geometry Change Methods

        // Start signal of a geometrychange for the viewable gameobjects
        public void BaseGhost_Finish_Pre(BaseGhost baseGhost)
        {
#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("BaseGhost_Finish_Pre");
#endif

            baseGhostIsFinished = true;
        }

        private string generateGameObjectTransformInfo(Transform transform)
        {
            return transform.name + " " + transform.parent.position.ToString() + " " + transform.GetSiblingIndex().ToString();
        }

        // Before the geometrychange we need to save the ids
        public void Base_ClearGeometry_Pre(Base @base)
        {
            if (baseGhostIsFinished)
            // Base_ClearGeometry_Pre is called twice. One time for the ghost layout and one time for the viewable layout. We are only interested in the viewable layout. 
            {
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

                            if (surface != null && surface.gameObject != null)
                            {

#if TRACE && BUILDING
                                string info = generateGameObjectTransformInfo(surface);
                                NitroxModel.Logger.Log.Debug("Base_ClearGeometry_Pre - current clear for : " + info);
#endif
                                if (surface.GetComponent<BaseDeconstructable>() != null && !surface.name.Contains("CorridorConnector"))

                                {

                                    NitroxId id = NitroxEntity.GetIdNullable(surface.gameObject);

                                    // Save only ids of already finished Objects that have a BaseDeconstructable attached, all other childs are Shape-Elements or not of interest
                                    // Have to ignore BaseRoomCorridorConnector, because they are automatically spawned in case of adding a Corridor or a Hatch to a Room
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
                                        NitroxModel.Logger.Log.Error("Base_ClearGeometry_Pre - no NitroxId found on GameObject: " + surface.name + " cellPosition: " + surface.parent.position + " surfaceIndex: " + surface.GetSiblingIndex());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // At respawning after geometrychange reassign the Ids

        public void BaseDeconstructable_MakeCellDeconstructable_Post(GameObject gameObject)
        {
            BaseDeconstructable_MakeDeconstructable_Post(gameObject.GetComponentInParent<Base>(), gameObject.transform);
        }

        public void BaseDeconstructable_MakeFaceDeconstructable_Post(GameObject gameObject)
        {
            BaseDeconstructable_MakeDeconstructable_Post(gameObject.GetComponentInParent<Base>(), gameObject.transform);
        }

        private void BaseDeconstructable_MakeDeconstructable_Post(Base @base, Transform surface)
        {
            if (baseGhostIsFinished)
            {

#if TRACE && BUILDING
                NitroxId baseId = NitroxEntity.GetIdNullable(@base.gameObject);
                NitroxId pieceId = NitroxEntity.GetIdNullable(surface.gameObject);
                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - base: " + @base + " baseId: " + baseId + " piece: " + surface.gameObject + " pieceID: " + pieceId + " piecePosition: " + surface.position + " pieceIndex: " + surface.GetSiblingIndex() + " pieceCellPosition: " + surface.parent.position + " pieceCellIndex: " + surface.parent.GetSiblingIndex());
#endif

                NitroxId id = NitroxEntity.GetIdNullable(surface.gameObject);
                if (id == null && surface.GetComponent<BaseDeconstructable>() != null && !surface.name.Contains("CorridorConnector"))
                {
                    // In case a vertical connector exists and a ladder is build inside the base, then this vertical connector changes its shape and name
                    if (surface.name.Contains("ConnectorTube"))
                    {
                        if (surface.name.Contains("Window"))
                        {
                            if (idCacheForGeometryUpdate.Exists(g => g.Name == surface.name.Replace("ConnectorTubeWindow", "ConnectorTube") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()))
                            {
                                id = idCacheForGeometryUpdate.Find(g => g.Name == surface.name.Replace("ConnectorTubeWindow", "ConnectorTube") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()).Id;

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface) + " id: " + id);
#endif

                                NitroxEntity.SetNewId(surface.gameObject, id);
                                return;
                            }
                        }
                        else
                        {
                            if (idCacheForGeometryUpdate.Exists(g => g.Name == surface.name.Replace("ConnectorTube", "ConnectorTubeWindow") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()))
                            {
                                id = idCacheForGeometryUpdate.Find(g => g.Name == surface.name.Replace("ConnectorTube", "ConnectorTubeWindow") && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()).Id;

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface) + " id: " + id);
#endif

                                NitroxEntity.SetNewId(surface.gameObject, id);
                                return;
                            }
                        }
                    }

                    // In other cases e.g adding rooms on top/bottom of each other or adding pieces that generate/remove stilts, the index of the childs is changed because surface are integrated/removed
                    if (surface.GetSiblingIndex() != 0 && idCacheForGeometryUpdate.Exists(g => g.CellPosition == surface.parent.position && g.CellIndex == 0))
                    {
                        GeometryInfo cachedInfo = idCacheForGeometryUpdate.Find(g => g.CellPosition == surface.parent.position && g.CellIndex == 0);
                        string currentParentContent = string.Empty;
                        for (int i = 0; i < surface.parent.childCount; i++)
                        {
                            currentParentContent = currentParentContent + surface.parent.GetChild(i).name;
                        }

                        // Check if the FaceIndex has moved. Multiple changes can happen at the same time. 
                        int moveFaceIndex = 0;

                        if (cachedInfo.CellContent.Contains("RoomExteriorTop") && !currentParentContent.Contains("RoomExteriorTop"))
                        // Case: A new room has been added to the top of an existing room, which removes the upper outer-transform of the existing room.
                        {
                            moveFaceIndex = moveFaceIndex + 1;
                        }
                        if (cachedInfo.CellContent.Contains("RoomExteriorBottom") && !currentParentContent.Contains("RoomExteriorBottom"))
                        // Case: A new room has been added to the bottom of an existing room.
                        {
                            moveFaceIndex = moveFaceIndex + 1;
                        }
                        if (cachedInfo.CellContent.Contains("AdjustableSupport") && !currentParentContent.Contains("AdjustableSupport"))
                        // Case: If a room or a corridor stands on stillts and a new room or corridor is placed below the existing one, the stillts get removed from the existing one. 
                        {
                            moveFaceIndex = moveFaceIndex + 1;
                        }
                        if (currentParentContent.Contains("RoomExteriorTop") && !cachedInfo.CellContent.Contains("RoomExteriorTop"))
                        // Case: Opposites of above.
                        {
                            moveFaceIndex = moveFaceIndex - 1;
                        }
                        if (currentParentContent.Contains("RoomExteriorBottom") && !cachedInfo.CellContent.Contains("RoomExteriorBottom"))
                        {
                            moveFaceIndex = moveFaceIndex - 1;
                        }
                        if (currentParentContent.Contains("AdjustableSupport") && !cachedInfo.CellContent.Contains("AdjustableSupport"))
                        {
                            moveFaceIndex = moveFaceIndex - 1;
                        }

                        if (moveFaceIndex != 0)
                        {
                            if (idCacheForGeometryUpdate.Exists(g => g.Name == surface.name && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex() + moveFaceIndex))
                            {
                                id = idCacheForGeometryUpdate.Find(g => g.Name == surface.name && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex() + moveFaceIndex).Id;

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface) + " id: " + id);
#endif

                                NitroxEntity.SetNewId(surface.gameObject, id);
                                return;
                            }
                        }
                    }

                    // At last try to find the object by its name, position and index if no changes have happened
                    if (idCacheForGeometryUpdate.Exists(g => g.Name == surface.name && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()))
                    {
                        id = idCacheForGeometryUpdate.Find(g => g.Name == surface.name && g.CellPosition == surface.parent.position && g.CellIndex == surface.GetSiblingIndex()).Id;

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface) + " id: " + id);
#endif

                        NitroxEntity.SetNewId(surface.gameObject, id);
                        return;
                    }

                    // If no cached id could be found, but we have a cachedID from a new Piece
                    if (transferIdforConstructableBaseConstructing != null)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - setting id for : " + generateGameObjectTransformInfo(surface) + " id: " + transferIdforConstructableBaseConstructing);
#endif
                        NitroxEntity.SetNewId(surface.gameObject, transferIdforConstructableBaseConstructing);


                        if (surface.name.Contains("BaseRoomWaterParkBottom") || surface.name.Contains("BaseRoomLadderTop") || surface.name.Contains("BaseCorridorLadderTop"))
                        // For WaterParks the BottomShape is always spawned first. For Ladders the TopShape is alwayse spawned first, no matter if build from the lower or upper floor.
                        {
                            if (!relationTopBottomIds.ContainsKey(transferIdforConstructableBaseConstructing))
                            {
                                NitroxId nitroxIdforOtherPart = new NitroxId();
                                relationTopBottomIds.Add(transferIdforConstructableBaseConstructing, nitroxIdforOtherPart);
                                transferIdforConstructableBaseConstructing = nitroxIdforOtherPart;

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Base_SpawnPiece_Post - caching a new Id for the other Top or Bottom part - cachedPieceIdFromConstructableBasePre: " + transferIdforConstructableBaseConstructing);
#endif

                            }
                            else
                            {
                                transferIdforConstructableBaseConstructing = null;
                            }
                        }
                        else
                        {
                            transferIdforConstructableBaseConstructing = null;
                        }
                    }
                }
            }
        }

        // End signal of of a geometrychange for the viewable gameobjects
        public void BaseGhost_Finish_Post(BaseGhost instance)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("BaseGhost_Finish_Post");
#endif

            baseGhostIsFinished = false;
            idCacheForGeometryUpdate.Clear();
        }

        // For Base objects we also need to transfer the ids
        public void Base_CopyFrom_Pre(Base instance, Base sourceBase)
        {
            NitroxId sourceBaseId = NitroxEntity.GetIdNullable(sourceBase.gameObject);
            NitroxId targetBaseId = NitroxEntity.GetIdNullable(instance.gameObject);

#if TRACE && BUILDING
            BaseRoot sourceBaseRoot = sourceBase.GetComponent<BaseRoot>();
            BaseRoot targetBaseRoot = instance.GetComponent<BaseRoot>();
            NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - Base copy - sourceBase: " + sourceBase + " targetBase: " + instance + " targetBaseIsGhost: " + instance.isGhost + " sourceBaseId: " + sourceBaseId + " targetBaseId: " + targetBaseId + " sourceBaseRoot: " + sourceBaseRoot + " targetBaseRoot: " + targetBaseRoot);
#endif

            if (baseGhostsIDCache.ContainsKey(sourceBase.gameObject) && targetBaseId == null && !instance.isGhost)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - assigning cached Id from remote event or initial loading: " + baseGhostsIDCache[sourceBase.gameObject]);
#endif

                NitroxEntity.SetNewId(instance.gameObject, baseGhostsIDCache[sourceBase.gameObject]);
            }
            else
            {
                if (sourceBaseId != null && targetBaseId == null && !instance.isGhost)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - assining id from local constructing of a new BaseComplex: " + sourceBaseId);
#endif

                    NitroxEntity.SetNewId(instance.gameObject, sourceBaseId);
                }
            }
        }

        // Memorize the id from an object that gets to 100% finished
        public bool Constructable_Construct_Pre(Constructable instance)
        {

            NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_Construct_Pre - instance: " + instance + " id: " + id + " construced: " + instance._constructed + " amount: " + instance.constructedAmount + " remoteEventActive: " + remoteEventActive);
#endif
            if (id != null)
            {
                transferIdforConstructableBaseConstructing = id;
                NitroxEntity.RemoveId(instance.gameObject);
            }

            return true;
        }

        // Memorize the id in case of deconstructing a already finished piece
        public void BaseDeconstructable_Deconstruct_Pre(BaseDeconstructable instance)
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
                            transferIdforConstructableBaseDeconstructing = item.Key;
                            NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(item.Key).Value);
                            NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(item.Value).Value);
                            break;
                        }
                    }
                }
                else
                {
                    transferIdforConstructableBaseDeconstructing = id;
                    NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(id).Value);
                }

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseDeconstructable_Deconstruct_Pre - saving guid from instance: " + instance.name + " cachedPieceIdFromConstructableBaseFinished: " + transferIdforConstructableBaseDeconstructing);
#endif
            }
        }

        // Reassign the cached id to the now constructable object
        public bool ConstructableBase_SetState_Pre(ConstructableBase instance, bool value, bool setAmount)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("ConstructableBase_SetState_Pre - instance: " + instance.name + " _constructed: " + instance._constructed + " value: " + value + " setAmount: " + setAmount + " instance.gameObject: " + instance.gameObject);
#endif

            if (value == false && setAmount == false && instance._constructed)
            {
                if (transferIdforConstructableBaseDeconstructing != null)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("ConstructableBase_SetState_Pre - setting id cachedPieceIdFromConstructableBaseFinished: " + transferIdforConstructableBaseDeconstructing);
#endif

                    NitroxEntity.SetNewId(instance.gameObject, transferIdforConstructableBaseDeconstructing);
                    transferIdforConstructableBaseDeconstructing = null;
                }
            }
            return true;
        }


        // Section: BuilderTool

        public void BuilderTool_OnHoverConstructable_Post(GameObject gameObject, Constructable constructable)
        {

#if TRACE && BUILDING && HOVERCONSTRUCTABLE
            NitroxId id = NitroxEntity.GetIdNullable(constructable.gameObject);
            NitroxModel.Logger.Log.Debug("BuilderTool_OnHoverConstructable_Post - instance: " + constructable.gameObject.name + " id: " + id);
#endif

            lastHoveredConstructable = constructable;
        }

        public void BuilderTool_OnHoverDeconstructable_Post(GameObject gameObject, BaseDeconstructable deconstructable)
        {

#if TRACE && BUILDING && HOVERDECONSTRUCTABLE
            NitroxId id = NitroxEntity.GetIdNullable(deconstructable.gameObject);
            NitroxId baseId = null;
            Base abase = deconstructable.gameObject.GetComponentInParent<Base>();
            if (abase)
            {
                baseId = NitroxEntity.GetIdNullable(abase.gameObject);
            }
            NitroxModel.Logger.Log.Debug("BuilderTool_OnHoverDeconstructable_Post - instance: " + deconstructable.gameObject.name + " id: " + id + " baseId: " + baseId + " position: " + deconstructable.gameObject.transform.position + " rotation: " + deconstructable.gameObject.transform.rotation + " cellPosition: " + deconstructable.gameObject.transform.parent.position + " cellIndex: " + deconstructable.gameObject.transform.GetSiblingIndex());
#endif

        }

        // Besides switching the state of currently handling a builderTool, this method is also intended to precheck if a construction/action even is allowed. If a 
        // remote player is currently using a gameobject (e.g. Fabricator) or is too using the buildertool on the same object, we want to deny the local action here 
        // and give a info to the player. 
        public bool BuilderTool_HandleInput_Pre(GameObject gameObject)
        {

#if TRACE && BUILDING && HOVER
            NitroxModel.Logger.Log.Debug("BuilderTool_Pre_HandleInput");
#endif

            currentlyHandlingBuilderTool = true;
            return true;

            /*
             * #TODO BUILDING# #ISSUE# Lock objects that are currently targeted by a player to be not constructed/deconstructed by others. 
             * 
            if (lastHoveredConstructable != null)
            {
                string _crafterGuid = GuidHelper.GetGuid(lastHoveredConstructable.gameObject);
                ushort _remotePlayerId;
                if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(_crafterGuid, out _remotePlayerId))
                {
                    //if Object is in use by remote Player, supress deconstruction
                    if (GameInput.GetButtonHeld(GameInput.Button.LeftHand) || GameInput.GetButtonDown(GameInput.Button.LeftHand) || GameInput.GetButtonHeld(GameInput.Button.Deconstruct) || GameInput.GetButtonDown(GameInput.Button.Deconstruct))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }*/
        }

        public void BuilderTool_HandleInput_Post(BuilderTool instance)
        {
            currentlyHandlingBuilderTool = false;
        }


        // SECTION: Local Events

        public void Constructable_Construct_Post(Constructable instance, bool result)
        {

#if TRACE && BUILDING
            NitroxId tempId = NitroxEntity.GetIdNullable(instance.gameObject);            
            NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - instance: " + instance + " tempId: " + tempId + " construced: " + instance._constructed + " amount: " + instance.constructedAmount + " remoteEventActive: " + remoteEventActive);
#endif

            //Check if we raised the event by using our own BuilderTool or if it came as post Event of a Remote-Action or Init-Action
            if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool)
            {

                if (result && instance.constructedAmount < 1f)
                {
                    //Send every event, because recourses used for construction (or gained for deconstruction) are else not properly taken or credited, 
                    //because we don't know exactly at which percentage-rate they are needed for every single item. Transfered packets are tiny and 
                    //building something is only a small amount of play time.
                    //Positive side effect is, that the process becomes smoother at the remote player side. 
                    /*if (timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
                    {
                        return;
                    }
                    timeSinceLastConstructionChangeEvent = 0.0f;*/

                    NitroxId id = NitroxEntity.GetId(instance.gameObject);
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - sending notify for self constructing object - id: " + id + " amount: " + instance.constructedAmount);
#endif
                    BaseConstructionAmountChanged amountChanged = new BaseConstructionAmountChanged(id, instance.constructedAmount, true);
                    packetSender.Send(amountChanged);
                }
            }

            //
            if (result && instance.constructedAmount == 1f && remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - finished construct remote");
#endif


                if (instance.gameObject.name.Contains("Solar") || instance.gameObject.name.Contains("Reactor"))
                {


#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - Energy");
#endif

                    if (instance.gameObject.name.Contains("Solar"))
                    {
                        PowerSource _powersource = instance.gameObject.GetComponent<PowerSource>();

                        if (_powersource)
                        {
#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - Energy _powersource: " + _powersource);
#endif
                            //TODO: Remind for later
                            //initialize with 50% after RemoteConstruction >> ToDo: use additional flag for only setting at initalsync or update to explicit value when energy is synced 
                            _powersource.SetPower(_powersource.maxPower / 2);
                        }
                    }
                }
            }
        }

        public void Constructable_Deconstruct_Post(Constructable instance, bool result)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - _construced: " + instance._constructed + " amount: " + instance.constructedAmount);
#endif

            //Check if we raised the event by using our own BuilderTool or if it came as post Event of a Remote-Action or Init-Action
            if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool)
            {
                NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                if (id == null)
                {
                    NitroxModel.Logger.Log.Error("Constructable_Deconstruct_Post - Trying to deconstruct an Object that has no NitroxId - gameObject: " + instance.gameObject);
                }
                else
                {
                    if (result && instance.constructedAmount <= 0f)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructed object - id: " + id);
#endif
                        BaseDeconstructionCompleted deconstructionCompleted = new BaseDeconstructionCompleted(id);
                        packetSender.Send(deconstructionCompleted);

                        if (relationTopBottomIds.ContainsKey(id))
                        {
                            if (NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).HasValue)
                            {
                                NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).Value);
                            }
                            relationTopBottomIds.Remove(id);
                        }
                        NitroxEntity.RemoveId(instance.gameObject);

                    }
                    else if (result && instance.constructedAmount > 0f)
                    {
                        //Send every event, because recourses used for construction (or gained for deconstruction) are else not properly taken or credited, 
                        //because we don't know exactly at which percentage-rate they are needed for every single item. Transfered packets are tiny and 
                        //building something is only a small amount of play time.
                        //Positive side effect is, that the process becomes smoother at the remote player side. 
                        /*timeSinceLastConstructionChangeEvent += Time.deltaTime;
                        if (timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
                        {
                            return;
                        }
                        timeSinceLastConstructionChangeEvent = 0.0f;*/


#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructing object  - id: " + id + " amount: " + instance.constructedAmount);
#endif
                        BaseConstructionAmountChanged amountChanged = new BaseConstructionAmountChanged(id, instance.constructedAmount, false);
                        packetSender.Send(amountChanged);
                    }
                }
            }
        }


        public void Constructable_NotifyConstructedChanged_Post(Constructable instance)
        {
            NitroxId tempId = NitroxEntity.GetIdNullable(instance.gameObject);
#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - instance: " + instance + " id: " + tempId + " _construced: " + instance._constructed + " amount: " + instance.constructedAmount);
#endif

            if (!remoteEventActive)
            {
#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - no remoteAction");
#endif

                // Case: A new base piece has been build by player
                if (!instance._constructed && instance.constructedAmount == 0f)
                {
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - case new instance");
#endif
                    if (!(instance is ConstructableBase))
                    {

                        NitroxId id = NitroxEntity.GetId(instance.gameObject);

                        NitroxId parentId = null;

                        SubRoot sub = Player.main.currentSub;

                        if (sub != null)
                        {
                            parentId = NitroxEntity.GetId(sub.gameObject);
                        }
                        else
                        {
                            Base playerBase = instance.gameObject.GetComponentInParent<Base>();

                            if (playerBase != null)
                            {
                                parentId = NitroxEntity.GetId(playerBase.gameObject);
                            }
                        }

                        Transform camera = Camera.main.transform;
                        BasePiece basePiece = new BasePiece(id, instance.gameObject.transform.position, instance.gameObject.transform.rotation, camera.position, camera.rotation, instance.techType.Model(), Optional.OfNullable(parentId), true, Optional.Empty);

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin constructing object - basePiece: " + basePiece );
#endif

                        BaseConstructionBegin constructionBegin = new BaseConstructionBegin(basePiece);
                        packetSender.Send(constructionBegin);
                    }
                    else
                    {
                        if (instance is ConstructableBase)
                        {
                            NitroxId parentBaseId = null;

                            BaseGhost ghost = instance.GetComponentInChildren<BaseGhost>();
                            if (ghost != null)
                            {
#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - creating constructable base with ghost: " + ghost);
#endif

                                if (ghost.TargetBase != null)
                                {
                                    // Case: a constructableBase is build in range of 3 cells to an existing base structure
#if TRACE && BUILDING
                                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost has target base: " + ghost.TargetBase);
#endif

                                    parentBaseId = NitroxEntity.GetIdNullable(ghost.TargetBase.gameObject);
                                    if (parentBaseId != null)
                                    {
#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - target base id: " + parentBaseId);
#endif
                                    }
                                    else
                                    {
                                        parentBaseId = NitroxEntity.GetId(ghost.TargetBase.gameObject);
#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - target base had no id, assigned new one: " + parentBaseId);
#endif
                                    }
                                }
                                else
                                {
#if TRACE && BUILDING
                                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost has no target base");
#endif
                                    if (ghost.GhostBase != null)
                                    {
                                        // Case: a constructableBase is build out of range of 3 cells of an existing base structure and is creating a new base complex

#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost has ghost base: " + ghost.GhostBase);
#endif

                                        parentBaseId = NitroxEntity.GetIdNullable(ghost.GhostBase.gameObject);
                                        if (parentBaseId != null)
                                        {
#if TRACE && BUILDING
                                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost base id: " + parentBaseId);
#endif
                                        }
                                        else
                                        {
                                            parentBaseId = NitroxEntity.GetId(ghost.GhostBase.gameObject);
#if TRACE && BUILDING
                                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost base had no id, assigned new one: " + parentBaseId);
#endif

                                        }
                                    }
                                    else
                                    {
#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost has no ghostbase and no targetbase");
#endif

                                        // Trying to find a Base in the parents of the ghost
                                        Base aBase = ghost.gameObject.GetComponentInParent<Base>();
                                        if (aBase != null)
                                        {
#if TRACE && BUILDING
                                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost has base in parentComponents: " + aBase);
#endif
                                            parentBaseId = NitroxEntity.GetIdNullable(aBase.gameObject);
                                            if (parentBaseId != null)
                                            {
#if TRACE && BUILDING
                                                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost parentComponents base id: " + parentBaseId);
#endif
                                            }
                                            else
                                            {
                                                parentBaseId = NitroxEntity.GetId(aBase.gameObject);
#if TRACE && BUILDING
                                                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost parentComponentsbase had no id, assigned new one: " + parentBaseId);
#endif

                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Case: the constructableBase doesn't use a ghostModel to be build, instead using its final objectModel to be build

#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - creating constructablebase without a ghost");
#endif

                                // Trying to find a Base in the parents of the gameobject itself
                                Base aBase = instance.gameObject.GetComponentInParent<Base>();
                                if (aBase != null)
                                {
#if TRACE && BUILDING
                                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - constructableBase has base in parentComponents: " + aBase);
#endif
                                    parentBaseId = NitroxEntity.GetIdNullable(aBase.gameObject);
                                    if (parentBaseId != null)
                                    {
#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - constructableBase parentComponents base id: " + parentBaseId);
#endif
                                    }
                                    else
                                    {
                                        parentBaseId = NitroxEntity.GetId(aBase.gameObject);
#if TRACE && BUILDING
                                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - constructableBase parentComponentsbase had no id, assigned new one: " + parentBaseId);
#endif
                                    }
                                }
                            }

                            Vector3 placedPosition = instance.gameObject.transform.position;

                            NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                            if (id == null)
                            {

                                id = NitroxEntity.GetId(instance.gameObject);
#if TRACE && BUILDING
                                NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - constructableBase gameobject had no id, assigned new one: " + id);
#endif

                            }

                            Transform camera = Camera.main.transform;
                            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(ghost);

#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - techType: " + instance.techType + " techType.Model(): " + instance.techType.Model());
#endif

                            //fix for wrong techType
                            TechType _orig = instance.techType;
                            if (_orig == TechType.BaseCorridor)
                            {
                                _orig = TechType.BaseConnector;
                            }
                            NitroxModel.DataStructures.TechType _techType = _orig.Model();

#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - _techType: " + _techType);
#endif

                            BasePiece basePiece = new BasePiece(id, placedPosition, instance.gameObject.transform.rotation, camera.position, camera.rotation, _techType, Optional.OfNullable(parentBaseId), false, rotationMetadata);

#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin constructing object - basePiece: " + basePiece);
#endif

                            BaseConstructionBegin constructionBegin = new BaseConstructionBegin(basePiece);
                            packetSender.Send(constructionBegin);
                        }
                    }
                }
                // Case: A local constructed item has been finished
                else if (instance._constructed && instance.constructedAmount == 1f)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - case item finished - lastHoveredConstructable: " + lastHoveredConstructable + " instance: " + instance + " currentlyHandlingBuilderTool: " + currentlyHandlingBuilderTool);
#endif
                    if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool)
                    {

                        NitroxId id = NitroxEntity.GetId(instance.gameObject);
                        Base parentBase = instance.gameObject.GetComponentInParent<Base>();
                        NitroxId parentBaseId = null;
                        if (parentBase != null)
                        {
                            parentBaseId = NitroxEntity.GetId(parentBase.gameObject);
                        }

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self end constructed object - id: " + id + " parentbaseId: " + parentBaseId);
#endif
                        BaseConstructionCompleted constructionCompleted = new BaseConstructionCompleted(id, parentBaseId);
                        packetSender.Send(constructionCompleted);
                    }
                    else
                    {

#if TRACE && BUILDING
                        NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - end of construction of - gameobject: " + instance.gameObject + " id: " + id);
#endif
                    }
                }
                //case: A finished item was started to be deconstructed by the local player
                else if (!instance._constructed && instance.constructedAmount == 1f)
                {
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - case item deconstruct");
#endif

                    NitroxId id = NitroxEntity.GetId(instance.gameObject);

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin deconstructing object - id: " + id);
#endif

                    BaseDeconstructionBegin deconstructionBegin = new BaseDeconstructionBegin(id);
                    packetSender.Send(deconstructionBegin);
                }

                lastHoveredConstructable = null;

            }
        }


        // SECTION: Remote events from Initial-Sync or remote players

        internal void Constructable_ConstructionBegin_Remote(BasePiece basePiece)
        {
#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - id: " + basePiece.Id + " parentbaseId: " + basePiece.ParentId  + " techType: " + basePiece.TechType + " basePiece: " + basePiece);
#endif

            remoteEventActive = true;
            try
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - techTypeEnum: " + basePiece.TechType.Enum());
#endif

                GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.Enum());

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - buildPrefab: " + buildPrefab);
#endif

                MultiplayerBuilder.overridePosition = basePiece.ItemPosition;
                MultiplayerBuilder.overrideQuaternion = basePiece.Rotation;
                MultiplayerBuilder.overrideTransform = new GameObject().transform;
                MultiplayerBuilder.overrideTransform.position = basePiece.CameraPosition;
                MultiplayerBuilder.overrideTransform.rotation = basePiece.CameraRotation;
                MultiplayerBuilder.placePosition = basePiece.ItemPosition;
                MultiplayerBuilder.placeRotation = basePiece.Rotation;
                MultiplayerBuilder.rotationMetadata = basePiece.RotationMetadata;

                if (!MultiplayerBuilder.Begin(buildPrefab))
                {
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Error("Constructable_ConstructionBegin_Remote - Cannot build Objekt: " + buildPrefab + ". Enforcing placement.");
#endif
                }

                GameObject parentBase = null;

                if (basePiece.ParentId.HasValue)
                {
                    parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrElse(null);
                    // In case of the first piece of a newly constructed Base from a remote Player or at InitialSync
                    // the ParentId has a Value, but the Id belongs to the BaseGhost instead of any known NitroxEntity.
                    // parentBase will be null, let this untouched to let the Multiplayer-Builder generate a ghost and
                    // assign the Id afterwards. 
                }

                Constructable constructable;
                GameObject gameObject;

                if (basePiece.IsFurniture)
                {
                    SubRoot subRoot = (parentBase != null) ? parentBase.GetComponent<SubRoot>() : null;
                    gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                    constructable = gameObject.RequireComponentInParent<Constructable>();

                }
                else
                {
                    constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                    gameObject = constructable.gameObject;
                    BaseGhost ghost = constructable.GetComponentInChildren<BaseGhost>();

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - ghost: " + ghost + " parentBaseID: " + basePiece.ParentId + " parentBase: " + parentBase);
                    if(ghost!=null)
                    {
                        NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - ghost.TargetBase: " + ghost.TargetBase + " ghost.GhostBase: " + ghost.GhostBase + " ghost.GhostBase.GameObject: " + ghost.GhostBase.gameObject);
                    }
#endif 

                    if (parentBase == null && basePiece.ParentId.HasValue && ghost != null && ghost.GhostBase != null && ghost.TargetBase == null)
                    {
                        // A new Base is created, transfer the Id to the ghost. 
                        // It will be reused to the finished base by the Base_CopyFrom_Patch.

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - setting new Base Id to ghostBase: " + ghost.GhostBase.gameObject + " parentBaseID: " + basePiece.ParentId.Value);
#endif 
                        baseGhostsIDCache[ghost.GhostBase.gameObject] = basePiece.ParentId.Value;

                    }
                }

                NitroxEntity.SetNewId(gameObject, basePiece.Id);

                /**
                 * Manually call start to initialize the object as we may need to interact with it within the same frame.
                 */
                System.Reflection.MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Validate.NotNull(startCrafting);
                startCrafting.Invoke(constructable, new object[] { });

            }
            finally
            {
                remoteEventActive = false;
            }
        }

        internal void Constructable_AmountChanged_Remote(NitroxId id, float constructionAmount, bool construct)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_AmountChanged_Remote - id: " + id + " amount: " + constructionAmount + " construct: " + construct);
#endif

            remoteEventActive = true;
            try
            {
                GameObject constructingGameObject = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (constructingGameObject == null)
                {
                    NitroxModel.Logger.Log.Error("Constructable_AmountChanged_Remote - received AmountChange for unknown id: " + id + " amount: " + constructionAmount + " construct: " + construct);
                    remoteEventActive = false;
                    return;
                }

                if (constructionAmount > 0.05f && constructionAmount < 0.95f)
                {
                    Constructable constructable = constructingGameObject.GetComponentInChildren<Constructable>();
                    constructable.constructedAmount = constructionAmount;
                    if (construct)
                    {
                        constructable.Construct();
                    }
                    else
                    {
                        constructable.Deconstruct();
                    }
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        internal void Constructable_ConstructionCompleted_Remote(NitroxId id, NitroxId parentbaseId)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_ConstructionCompleted_Remote - id: " + id + " parentbaseId: " + parentbaseId);
#endif

            remoteEventActive = true;
            try
            {

                GameObject constructingGameObject = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (constructingGameObject == null)
                {
                    NitroxModel.Logger.Log.Error("Constructable_ConstructionCompleted_Remote - received ConstructionComplete for unknown id: " + id + " parentbaseID: " + parentbaseId);
                    remoteEventActive = false;
                    return;
                }

                ConstructableBase constructableBase = constructingGameObject.GetComponent<ConstructableBase>();
                if (constructableBase)
                {
                    constructableBase.constructedAmount = 1f;
                    constructableBase.Construct();
                }
                else
                {
                    Constructable constructable = constructingGameObject.GetComponent<Constructable>();
                    if (constructable)
                    {
                        constructable.constructedAmount = 1f;
                        constructable.Construct();
                        constructable.SetState(true, true);
                    }
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        internal void Constructable_DeconstructionBegin_Remote(NitroxId id)
        {
            remoteEventActive = true;

            try
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_DeconstructionBegin_Remote - id: " + id);
#endif

                GameObject deconstructing = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (deconstructing == null)
                {
                    NitroxModel.Logger.Log.Error("Constructable_ConstructionCompleted_Remote - received DeconstructionBegin for unknown id: " + id);
                    remoteEventActive = false;
                    return;
                }

                BaseDeconstructable baseDeconstructable = deconstructing.GetComponent<BaseDeconstructable>();
                if (baseDeconstructable)
                {
                    baseDeconstructable.Deconstruct();
                }
                else
                {
                    Constructable constructable = deconstructing.RequireComponent<Constructable>();
                    constructable.SetState(false, false);
                    constructable.Deconstruct();
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        internal void Constructable_DeconstructionComplete_Remote(NitroxId id)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_DeconstructionComplete_Remote - id: " + id);
#endif

            remoteEventActive = true;
            try
            {
                GameObject deconstructing = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (deconstructing == null)
                {
                    NitroxModel.Logger.Log.Error("Constructable_DeconstructionComplete_Remote - received DeconstructionComplete for unknown id: " + id);
                    remoteEventActive = false;
                    return;
                }

                if (relationTopBottomIds.ContainsKey(id))
                {
                    if (NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).HasValue)
                    {
                        NitroxEntity.RemoveId(NitroxEntity.GetObjectFrom(relationTopBottomIds[id]).Value);
                    }
                    relationTopBottomIds.Remove(id);
                }

                NitroxEntity.RemoveId(deconstructing);

                ConstructableBase constructableBase = deconstructing.GetComponent<ConstructableBase>();
                if (constructableBase)
                {
                    constructableBase.constructedAmount = 0f;
                    constructableBase.Deconstruct();
                }
                else
                {
                    Constructable constructable = deconstructing.GetComponent<Constructable>();
                    constructable.constructedAmount = 0f;
                    constructable.Deconstruct();
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }


        internal class GeometryInfo
        {
            internal string Name;
            internal NitroxId Id;
            internal Vector3 CellPosition;
            internal int CellIndex;
            internal string CellContent;
        }


        // SECTION: For tracing purposes. Will be removed when functionality is fully verified. 

        public void BaseRoot_Constructor_Post(BaseRoot instance)
        {
            if (instance.isBase)
            {
                NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);


#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseRoot_Constructor_Post - New BaseRoot Instance - instance: " + instance + " instance.gameObject: " + instance.gameObject + " gameObjectId: " + id );
#endif

            }
        }

        public bool CellManager_RegisterEntity_Pre(GameObject baseEntity)
        {

#if TRACE && BUILDING
            if (remoteEventActive)
            {
                NitroxModel.Logger.Log.Debug("CellManager_RegisterEntity_Pre - instance: " + baseEntity + " instance.name: " + baseEntity.name);
            }
#endif

            return true;
        }
    }
}
