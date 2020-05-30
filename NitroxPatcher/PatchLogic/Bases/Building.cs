using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxPatcher.PatchLogic.Bases
{
    public class Building : IBuilding
    {
        // State if currently in InitialSync phase
        private bool isInitialSyncing = false;
        
        // Contains the last hovered constructable object hovered by the current player. Is needed to ensure fired events for the correct item.
        private Constructable lastHoveredConstructable = null;

        // State of currently using BuilderTool or not
        private bool currentlyHandlingBuilderTool = false;

        // State if a construction Event is raised by Initialsync or a current remote player.
        private bool remoteEventActive = false;

        // For the base objects themself as master objects of a base-complex we can't assign Ids to the ghosts, 
        private Dictionary<GameObject, NitroxId> baseGhostsIDCache = new Dictionary<GameObject, NitroxId>();

        private Rotation.SubnauticaRotationMetadataFactory rotationMetadataFactory = new Rotation.SubnauticaRotationMetadataFactory();

        bool IBuilding.InitialSyncActive { set => isInitialSyncing = value; }

        void IBuilding.ConstructNewBasePiece(BasePiece basePiece)
        {
#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - id: " + basePiece.Id + " parentbaseId: " + basePiece.ParentId  + " techType: " + basePiece.TechType + " basePiece: " + basePiece);
#endif

            remoteEventActive = true;
            try
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - techTypeEnum: " + basePiece.TechType.ToUnity());
#endif

                GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.ToUnity());

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - buildPrefab: " + buildPrefab);
#endif

                MultiplayerBuilder.overridePosition = basePiece.ItemPosition.ToUnity();
                MultiplayerBuilder.overrideQuaternion = basePiece.Rotation.ToUnity();
                MultiplayerBuilder.overrideTransform = new GameObject().transform;
                MultiplayerBuilder.overrideTransform.position = basePiece.CameraPosition.ToUnity();
                MultiplayerBuilder.overrideTransform.rotation = basePiece.CameraRotation.ToUnity();
                MultiplayerBuilder.placePosition = basePiece.ItemPosition.ToUnity();
                MultiplayerBuilder.placeRotation = basePiece.Rotation.ToUnity();
                MultiplayerBuilder.rotationMetadata = basePiece.RotationMetadata;
                MultiplayerBuilder.IsInitialSyncing = isInitialSyncing;

                if (!MultiplayerBuilder.Begin(buildPrefab))
                {
                    Log.Error("Initial or Remote construction of a new Object failed: " + buildPrefab + " id: " + basePiece.Id);

                    MultiplayerBuilder.End();
                    return;
                }

                GameObject parentBase = null;

                if (basePiece.ParentId.HasValue)
                {
                    parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrElse(null);
                    // In case of the first piece of a newly constructed Base from a remote Player or at InitialSync
                    // the ParentId has a Value, but the Id belongs to the BaseGhost instead of any known NitroxEntity.
                    // ParentBase will be null, let this untouched to let the Multiplayer-Builder generate a ghost and
                    // assign the Id afterwards. 
                }

                Constructable constructable;
                GameObject gameObject;

                if (basePiece.IsFurniture)
                {
                    SubRoot subRoot = (parentBase != null) ? parentBase.GetComponent<SubRoot>() : null;
                    gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
                    constructable = gameObject.RequireComponentInParent<Constructable>();
                    NitroxEntity.SetNewId(gameObject, basePiece.Id);
                }
                else
                {
                    // Clear the cache, in case, the last constructed object wasn't finished and a former id is still cached
                    NitroxServiceLocator.LocateService<GeometryLayoutChangeHandler>().ClearPreservedIdForConstructing();

                    constructable = MultiplayerBuilder.TryPlaceBase(parentBase);
                    gameObject = constructable.gameObject;
                    NitroxEntity.SetNewId(gameObject, basePiece.Id);
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

                // Initialization of the ressourceMap of the constructable.
                System.Reflection.MethodInfo initResourceMap = typeof(Constructable).GetMethod("InitResourceMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Validate.NotNull(initResourceMap);
                initResourceMap.Invoke(constructable, new object[] { });
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        void IBuilding.ChangeConstructAmount(NitroxId id, float constructionAmount)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_AmountChanged_Remote - id: " + id + " amount: " + constructionAmount);
#endif

            remoteEventActive = true;
            try
            {
                GameObject constructingGameObject = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (constructingGameObject == null)
                {
                    Log.Error("Constructable_AmountChanged_Remote - received AmountChange for unknown id: " + id + " amount: " + constructionAmount);
                    remoteEventActive = false;
                    return;
                }

                if (constructionAmount > 0f && constructionAmount < 1f)
                {
                    Constructable constructable = constructingGameObject.GetComponentInChildren<Constructable>();
                    if (constructable.constructedAmount < constructionAmount)
                    {
                        constructable.constructedAmount = constructionAmount;
                        constructable.Construct();
                    }
                    else
                    {
                        constructable.constructedAmount = constructionAmount;
                        constructable.Deconstruct();
                    }
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        void IBuilding.FinishConstruction(NitroxId id)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_ConstructionCompleted_Remote - id: " + id);
#endif

            remoteEventActive = true;
            try
            {

                GameObject constructingGameObject = NitroxEntity.GetObjectFrom(id).OrElse(null);

                if (constructingGameObject == null)
                {
                    Log.Error("Constructable_ConstructionCompleted_Remote - received ConstructionComplete for unknown id: " + id);
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
                    }
                }
            }
            finally
            {
                remoteEventActive = false;
            }
        }

        void IBuilding.DeconstructBasePiece(NitroxId id)
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
                    Log.Error("Constructable_ConstructionCompleted_Remote - received DeconstructionBegin for unknown id: " + id);
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

        void IBuilding.FinishDeconstruction(NitroxId id)
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
                    Log.Error("Constructable_DeconstructionComplete_Remote - received DeconstructionComplete for unknown id: " + id);
                    remoteEventActive = false;
                    return;
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


        // For Base objects we need to transfer the ids
        public void Base_CopyFrom_Pre(Base targetBase, Base sourceBase)
        {
            NitroxId sourceBaseId = NitroxEntity.GetIdNullable(sourceBase.gameObject);
            NitroxId targetBaseId = NitroxEntity.GetIdNullable(targetBase.gameObject);

#if TRACE && BUILDING
            BaseRoot sourceBaseRoot = sourceBase.GetComponent<BaseRoot>();
            BaseRoot targetBaseRoot = targetBase.GetComponent<BaseRoot>();
            NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - Base copy - sourceBase: " + sourceBase + " targetBase: " + targetBase + " targetBaseIsGhost: " + targetBase.isGhost + " sourceBaseId: " + sourceBaseId + " targetBaseId: " + targetBaseId + " sourceBaseRoot: " + sourceBaseRoot + " targetBaseRoot: " + targetBaseRoot);
#endif

            if (baseGhostsIDCache.ContainsKey(sourceBase.gameObject) && targetBaseId == null && !targetBase.isGhost)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - assigning cached Id from remote event or initial loading: " + baseGhostsIDCache[sourceBase.gameObject]);
#endif

                NitroxEntity.SetNewId(targetBase.gameObject, baseGhostsIDCache[sourceBase.gameObject]);
            }
            // Transferring from a real base to a ghost base in case of beginning deconstruction of the last basepiece. Need this if player does not completely destroy 
            // last piece instead chooses to reconstruct this last piece.
            else if (sourceBaseId != null && !sourceBase.isGhost && !baseGhostsIDCache.ContainsKey(targetBase.gameObject))
            {
                baseGhostsIDCache[targetBase.gameObject] = sourceBaseId;

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - caching Base Id from deconstructing object: " + sourceBaseId);
#endif
            }
        }

        // Suppress item consumption and recalculation of construction amount at construction
        public bool Constructable_Construct_Pre(Constructable instance, ref bool result)
        {
            if (remoteEventActive)
            {
                if (instance.constructed)
                {
                    result = false;
                }
                else
                {
                    System.Reflection.MethodInfo updateMaterial = typeof(Constructable).GetMethod("UpdateMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Validate.NotNull(updateMaterial);
                    updateMaterial.Invoke(instance, new object[] { });
                    if (instance.constructedAmount >= 1f)
                    {
                        instance.SetState(true, true);
                    }
                    result = true;
                }
                return false;
            }
            return true;
        }

        // Suppress item granting and recalculation of construction amount at construction  and remove NitroxId from 
        public bool Constructable_Deconstruct_Pre(Constructable instance, ref bool result)
        {
            if (remoteEventActive)
            {
                if (instance.constructed)
                {
                    result = false;
                }
                else
                {
                    System.Reflection.MethodInfo updateMaterial = typeof(Constructable).GetMethod("UpdateMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    Validate.NotNull(updateMaterial);
                    updateMaterial.Invoke(instance, new object[] { });
                    if (instance.constructedAmount <= 0f)
                    {
                        UnityEngine.Object.Destroy(instance.gameObject);
                    }
                    result = true;
                }
                return false;
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
            if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool && !remoteEventActive)
            {

                if (result && instance.constructedAmount < 1f)
                {
                    NitroxId id = NitroxEntity.GetId(instance.gameObject);

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - sending notify for self constructing object - id: " + id + " amount: " + instance.constructedAmount);
#endif
                    BaseConstructionAmountChanged amountChanged = new BaseConstructionAmountChanged(id, instance.constructedAmount);
                    NitroxServiceLocator.LocateService<IPacketSender>().Send(amountChanged);
                }
            }

            if (result && instance.constructedAmount == 1f && remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_Construct_Post - finished construct remote");
#endif

            }
        }

        public void Constructable_Deconstruct_Post(Constructable instance, bool result)
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - _construced: " + instance._constructed + " amount: " + instance.constructedAmount);
#endif

            //Check if we raised the event by using our own BuilderTool or if it came as post Event of a Remote-Action or Init-Action
            if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool && !remoteEventActive)
            {
                NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                if (id == null)
                {
                    Log.Error("Constructable_Deconstruct_Post - Trying to deconstruct an Object that has no NitroxId - gameObject: " + instance.gameObject);
                }
                else
                {
                    if (result && instance.constructedAmount <= 0f)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructed object - id: " + id);
#endif
                        BaseDeconstructionCompleted deconstructionCompleted = new BaseDeconstructionCompleted(id);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(deconstructionCompleted);

                    }
                    else if (result && instance.constructedAmount > 0f)
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_Deconstruct_Post - sending notify for self deconstructing object  - id: " + id + " amount: " + instance.constructedAmount);
#endif
                        BaseConstructionAmountChanged amountChanged = new BaseConstructionAmountChanged(id, instance.constructedAmount);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(amountChanged);
                    }
                }
            }

            if (result && instance.constructedAmount <= 0f)
            {
                if (instance.gameObject)
                {
                    NitroxEntity.RemoveId(instance.gameObject);
                    UnityEngine.Object.Destroy(instance.gameObject);
                }
            }
        }


        public void Constructable_NotifyConstructedChanged_Post(Constructable instance)
        {

#if TRACE && BUILDING
            NitroxId tempId = NitroxEntity.GetIdNullable(instance.gameObject);
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
                            Base nearBase = instance.gameObject.GetComponentInParent<Base>();
                            if (nearBase != null)
                            {
                                parentId = NitroxEntity.GetId(nearBase.gameObject);
                            }
                        }

                        Transform camera = Camera.main.transform;
                        BasePiece basePiece = new BasePiece(id, instance.gameObject.transform.position.ToDto(), instance.gameObject.transform.rotation.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), instance.techType.ToDto(), Optional.OfNullable(parentId), true, Optional.Empty);

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin constructing object - basePiece: " + basePiece );
#endif

                        BaseConstructionBegin constructionBegin = new BaseConstructionBegin(basePiece);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(constructionBegin);
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
                                            parentBaseId = new NitroxId();
                                            baseGhostsIDCache[ghost.GhostBase.gameObject] = parentBaseId;

#if TRACE && BUILDING
                                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - ghost base had no id, cached new one: " + baseGhostsIDCache[ghost.GhostBase.gameObject]);
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
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - techType: " + instance.techType + " techType.Model(): " + instance.techType.ToDto());
#endif

                            //fix for wrong techType
                            TechType origTechType = instance.techType;
                            if (origTechType == TechType.BaseCorridor)
                            {
                                origTechType = TechType.BaseConnector;
                            }


#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - techType: " + origTechType);
#endif

                            BasePiece basePiece = new BasePiece(id, placedPosition.ToDto(), instance.gameObject.transform.rotation.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), origTechType.ToDto(), Optional.OfNullable(parentBaseId), false, rotationMetadata);

#if TRACE && BUILDING
                            NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin constructing object - basePiece: " + basePiece);
#endif

                            BaseConstructionBegin constructionBegin = new BaseConstructionBegin(basePiece);
                            NitroxServiceLocator.LocateService<IPacketSender>().Send(constructionBegin);
                        }
                    }
                }
                // Case: A local constructed item has been finished
                else if (instance._constructed && instance.constructedAmount == 1f)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - case item finished - lastHoveredConstructable: " + lastHoveredConstructable + " instance: " + instance + " currentlyHandlingBuilderTool: " + currentlyHandlingBuilderTool);
#endif
                    if (lastHoveredConstructable != null && lastHoveredConstructable == instance && currentlyHandlingBuilderTool && !remoteEventActive)
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
                        BaseConstructionCompleted constructionCompleted = new BaseConstructionCompleted(id);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(constructionCompleted);
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
                else if (!instance._constructed && instance.constructedAmount == 1f && !remoteEventActive)
                {
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - case item deconstruct");
#endif

                    NitroxId id = NitroxEntity.GetIdNullable(instance.gameObject);
                    if (id == null)
                    {
                        Log.Error("Constructable_NotifyConstructedChanged_Post - no id on local object - object: " + instance.gameObject);
                    }
                    else
                    {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin deconstructing object - id: " + id);
#endif

                        BaseDeconstructionBegin deconstructionBegin = new BaseDeconstructionBegin(id);
                        NitroxServiceLocator.LocateService<IPacketSender>().Send(deconstructionBegin);
                    }
                }

                lastHoveredConstructable = null;
            }
        }

        // Suppress hull integrity calculation on InitialSync
        public bool BaseHullStrength_OnPostRebuildGeometry_Pre(BaseHullStrength instance, Base b)
        {
            if (isInitialSyncing || remoteEventActive)
            {
#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_SetupGhost_Pre");
#endif
                if (GameModeUtils.RequiresReinforcements())
                {
                    float num = 10f;
                    ((List<LiveMixin>)instance.ReflectionGet("victims")).Clear();
                    foreach (Int3 cell in ((Base)instance.ReflectionGet("baseComp")).AllCells)
                    {
                        if (((Base)instance.ReflectionGet("baseComp")).GridToWorld(cell).y < 0f)
                        {
                            Transform cellObject = ((Base)instance.ReflectionGet("baseComp")).GetCellObject(cell);
                            if (cellObject != null)
                            {
                                ((List<LiveMixin>)instance.ReflectionGet("victims")).Add(cellObject.GetComponent<LiveMixin>());
                                num += ((Base)instance.ReflectionGet("baseComp")).GetHullStrength(cell);
                            }
                        }
                    }
                    if (!UnityEngine.Mathf.Approximately(num, (float)instance.ReflectionGet("totalStrength")))
                    {
                        if (!isInitialSyncing) // Display no Messages on Initialsync
                        {
                            // If remote player finished construction of a base structure, calculate the distance 
                            // and display the message only if remote player is near the lokal player.
                            // ## TODO BUILDING ##
                            // Calulate if near and then:
                            // ErrorMessage.AddMessage(Language.main.GetFormat<float, float>("BaseHullStrChanged", num - (float)instance.ReflectionGet("totalStrength"), num));
                        }
                    }
                    instance.ReflectionSet("totalStrength", num);
                }
                return false;
            }
            return true;
        }

        // Suppress rotation hints on InitialSync and build actions of remote player
        public bool Builder_ShowRotationControlsHint_Pre()
        {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_ShowRotationControlsHint_Pre - isInitialSyncing: " + IsInitialSyncing + " remoteEventActive: " + remoteEventActive);
#endif

            if (remoteEventActive)
            {
#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_ShowRotationControlsHint_Pre - returning false");
#endif
                return false;
            }
            return true;
        }

        // Suppress rotation hints on InitialSync and build actions of remote player
        public bool BaseAddModuleGhost_SetupGhost_Pre(BaseAddModuleGhost instance)
        {
            if (remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_SetupGhost_Pre");
#endif

                instance.ReflectionCall("UpdateSize", false, false, new object[] { Int3.one });
                instance.ReflectionSet("direction", Base.Direction.North);
                instance.ReflectionSet("directions", new List<Base.Direction>(Base.HorizontalDirections));

                return false;
            }
            return true;
        }


        // On constructing base pieces on from a remote player construction or initialsync, 
        // it is needed to skip and ignore the BuilderTool input handling of the local player.
        internal bool Builder_Update_Pre()
        {
            if (remoteEventActive)
            {
                typeof(Builder).GetMethod("Initialize", System.Reflection.BindingFlags.Static).Invoke(null, null);
                typeof(Builder).ReflectionSet("canPlace", false, true, true);
                if (typeof(Builder).ReflectionGet("prefab", false, true) == null)
                {
                    return true; //the true is for skipping the original method, as Builder.Update doesn't have a return value
                }
                if ((bool)typeof(Builder).GetMethod("CreateGhost", System.Reflection.BindingFlags.Static).Invoke(null, null))
                {
                    // skip original
                    //Builder.inputHandler.canHandleInput = true;
                    //InputHandlerStack.main.Push(Builder.inputHandler);
                }
                typeof(Builder).ReflectionSet("canPlace", (bool)typeof(Builder).GetMethod("UpdateAllowed", System.Reflection.BindingFlags.Static).Invoke(null, null), false, true);
                Transform transform = ((GameObject)typeof(Builder).ReflectionGet("ghostModel", false, true)).transform;
                transform.position = ((Vector3)typeof(Builder).ReflectionGet("placePosition", false, true)) + ((Quaternion)typeof(Builder).ReflectionGet("placeRotation", false, true)) * ((Vector3)typeof(Builder).ReflectionGet("ghostModelPosition", false, true));
                transform.rotation = ((Quaternion)typeof(Builder).ReflectionGet("placeRotation", false, true)) * ((Quaternion)typeof(Builder).ReflectionGet("ghostModelRotation", false, true));
                transform.localScale = ((Vector3)typeof(Builder).ReflectionGet("placeRotation", false, true));
                Color value = Builder.canPlace ? ((Color)typeof(Builder).ReflectionGet("placeColorAllow", false, true)) : ((Color)typeof(Builder).ReflectionGet("placeColorDeny", false, true));
                IBuilderGhostModel[] components = ((GameObject)typeof(Builder).ReflectionGet("ghostModel", false, true)).GetComponents<IBuilderGhostModel>();
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].UpdateGhostModelColor(Builder.canPlace, ref value);
                }
                ((Material)typeof(Builder).ReflectionGet("ghostStructureMaterial", false, true)).SetColor(ShaderPropertyID._Tint, value);

                return true; // return true to skip original
            }
            return false; // if local player does something, return false to let original method execute
        }
    }
}
