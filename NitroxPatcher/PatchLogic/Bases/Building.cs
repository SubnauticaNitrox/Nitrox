using System;
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
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.Helper.Int3;
using UnityEngine;

namespace NitroxPatcher.PatchLogic.Bases
{
    public class Building : IBuilding
    {
        #region Private Members

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

        private BasePiece currentConstructedNewBasePiece = null;
        private GameObject currentconstructedGameObject = null;

        #endregion

        #region IBuilding Implementation (remote events and initialsync)

        bool IBuilding.InitialSyncActive { set => isInitialSyncing = value; }

        void IBuilding.ConstructNewBasePiece(BasePiece basePiece)
        {
#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - id: " + basePiece.Id + " parentbaseId: " + basePiece.ParentId + " techType: " + basePiece.TechType + " basePiece: " + basePiece);
#endif

            remoteEventActive = true;
            try
            {
                currentConstructedNewBasePiece = basePiece;

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - techTypeEnum: " + basePiece.TechType.ToUnity());
#endif

                GameObject buildPrefab = CraftData.GetBuildPrefab(basePiece.TechType.ToUnity());

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - buildPrefab: " + buildPrefab);
#endif

                if (!Builder.Begin(buildPrefab))
                {
                    Log.Error("Creating GhostModel Object failed: " + buildPrefab + " id: " + basePiece.Id);

                    Builder.End();
                    return;
                }

                if (!Builder.canPlace)
                {
                    Log.Error("Object can not be placed: " + buildPrefab + " id: " + basePiece.Id);

                    Builder.End();
                    return;
                }

                if (!Builder.TryPlace())
                {
                    Log.Error("Placing of Object failed: " + buildPrefab + " id: " + basePiece.Id);
                    Builder.End();
                    return;
                }

                Builder.End();

                if (currentconstructedGameObject == null)
                {
                    Log.Error("ConstructedGameObject does not exist: " + buildPrefab + " id: " + basePiece.Id);
                    return;
                }

                NitroxEntity.SetNewId(currentconstructedGameObject, basePiece.Id);

                GameObject parentBase = null;
                if (basePiece.ParentId.HasValue)
                {
                    parentBase = NitroxEntity.GetObjectFrom(basePiece.ParentId.Value).OrElse(null);
                    // In case of the first piece of a newly constructed Base from a remote Player or at InitialSync
                    // the ParentId has a Value, but the Id belongs to the BaseGhost instead of any known NitroxEntity.
                    // ParentBase will be null, let this untouched to let the Multiplayer-Builder generate a ghost and
                    // assign the Id afterwards. 
                }

                if (!basePiece.IsFurniture)
                {
                    Constructable constructable = currentconstructedGameObject.GetComponent<Constructable>();

                    if (constructable != null)
                    {
                        BaseGhost ghost = constructable.GetComponentInChildren<BaseGhost>();

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_ConstructionBegin_Remote - ghost: " + ghost + " parentBaseID: " + basePiece.ParentId + " parentBase: " + parentBase);
                        if (ghost != null)
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

                        System.Reflection.MethodInfo initResourceMap = typeof(Constructable).GetMethod("InitResourceMap", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        Validate.NotNull(initResourceMap);
                        initResourceMap.Invoke(constructable, new object[] { });
                    }
                }
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

        #endregion

        #region Broadcast changes from local Player

        internal void Constructable_Construct_Post(Constructable instance, bool result)
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

        internal void Constructable_Deconstruct_Post(Constructable instance, bool result)
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

        internal void Constructable_NotifyConstructedChanged_Post(Constructable instance)
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
                        RotationMetadata rotationMetadata = null;
                        if (sub != null)
                        {
                            parentId = NitroxEntity.GetId(sub.gameObject);
                            if (sub.isCyclops)
                            {
                                rotationMetadata = new SubModuleRotationMetadata(sub.gameObject.transform.position.ToDto(), sub.gameObject.transform.rotation.ToDto());
                            }
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
                        BasePiece basePiece = new BasePiece(id, instance.gameObject.transform.position.ToDto(), instance.gameObject.transform.rotation.ToDto(), camera.position.ToDto(), camera.rotation.ToDto(), instance.techType.ToDto(), Optional.OfNullable(parentId), true, Optional.OfNullable(rotationMetadata));

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Constructable_NotifyConstructedChanged_Post - sending notify for self begin constructing object - basePiece: " + basePiece);
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

        #endregion

        #region BuilderTool handling

        internal void BuilderTool_OnHoverConstructable_Post(GameObject gameObject, Constructable constructable)
        {

#if TRACE && BUILDING && HOVERCONSTRUCTABLE
            NitroxId id = NitroxEntity.GetIdNullable(constructable.gameObject);
            NitroxModel.Logger.Log.Debug("BuilderTool_OnHoverConstructable_Post - instance: " + constructable.gameObject.name + " id: " + id);
#endif

            lastHoveredConstructable = constructable;
        }

        internal void BuilderTool_OnHoverDeconstructable_Post(GameObject gameObject, BaseDeconstructable deconstructable)
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
        internal bool BuilderTool_HandleInput_Pre(GameObject gameObject)
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

        internal void BuilderTool_HandleInput_Post(BuilderTool instance)
        {
            currentlyHandlingBuilderTool = false;
        }

        #endregion

        #region NitroxId transfer for Bases

        // For Base objects we need to transfer the ids
        internal void Base_CopyFrom_Pre(Base targetBase, Base sourceBase)
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
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - caching Base Id from targetBase: " + sourceBaseId);
#endif
            }
        }

        #endregion

        #region Suppress item consumtion/granting for remote events

        // Suppress item consumption and recalculation of construction amount at construction
        internal bool Constructable_Construct_Pre(Constructable instance, ref bool result)
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
        internal bool Constructable_Deconstruct_Pre(Constructable instance, ref bool result)
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

        #endregion

        #region Suppress hull update infos on initialsync

        // Suppress hull update infos on initialsync
        internal bool BaseHullStrength_OnPostRebuildGeometry_Pre(BaseHullStrength instance, Base b)
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

        #endregion

        #region Suppress rotation hints for remote player or initialsync

        // Suppress rotation hints on InitialSync and build actions of remote player
        internal bool Builder_ShowRotationControlsHint_Pre()
        {

#if TRACE && BUILDING
            NitroxModel.Logger.Log.Debug("Builder_ShowRotationControlsHint_Pre - isInitialSyncing: " + isInitialSyncing + " remoteEventActive: " + remoteEventActive);
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
        internal bool BaseAddModuleGhost_SetupGhost_Pre(BaseAddModuleGhost instance)
        {
            if (remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_SetupGhost_Pre");
#endif

                instance.ReflectionCall("UpdateSize", false, false, new object[] { Int3.one });
                instance.ReflectionSet("direction", Base.Direction.North);
                instance.ReflectionSet("directions", new List<Base.Direction>(Base.HorizontalDirections));

                // skip rotation hints

                return false;
            }
            return true;
        }

        #endregion

        #region Builder patches to allow multiplayer usage

        // On construction of a base piece that is initiated from a remote player or initialsync, 
        // it is needed to skip and ignore the BuilderTool input handling of the local player.
        internal bool Builder_Update_Pre()
        {
            if (remoteEventActive)
            {

                typeof(Builder).GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_Update_Pre - initialized");
#endif
                typeof(Builder).GetProperty("canPlace").SetValue(null, false, null);
#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_Update_Pre - canPlace: " + Builder.canPlace);
#endif
                if (typeof(Builder).GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) == null)
                {
                    return false; // the returned false is for skipping the original method, as Builder.Update doesn't have a return value
                }

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_Update_Pre - creating ghost");
#endif

                if ((bool)typeof(Builder).GetMethod("CreateGhost", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null))
                {
                    // suppress original code

                    //Builder.inputHandler.canHandleInput = true;
                    //InputHandlerStack.main.Push(Builder.inputHandler);

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Builder_Update_Pre - created ghost");
#endif

                }
                bool canPlace = (bool)typeof(Builder).GetMethod("UpdateAllowed", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
                typeof(Builder).GetProperty("canPlace").SetValue(null, canPlace, null);

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_Update_Pre - canPlace: " + Builder.canPlace);
#endif

                
                //  ## TODO BUILDING ##  Let this code stay to allow Issue analyze of provided save games to see where ghosts are tried to be placed
                /*
                Transform transform = ((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).transform;
                transform.position = ((Vector3)typeof(Builder).GetField("placePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)) + ((Quaternion)typeof(Builder).GetField("placeRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)) * ((Vector3)typeof(Builder).GetField("ghostModelPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null));
                transform.rotation = ((Quaternion)typeof(Builder).GetField("placeRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)) * ((Quaternion)typeof(Builder).GetField("ghostModelRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null));
                transform.localScale = ((Vector3)typeof(Builder).GetField("placeRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null));
                Color value = Builder.canPlace ? ((Color)typeof(Builder).GetField("placeColorAllow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)) : ((Color)typeof(Builder).GetField("placeColorDeny", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null));
                IBuilderGhostModel[] components = ((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)).GetComponents<IBuilderGhostModel>();
                for (int i = 0; i < components.Length; i++)
                {
                    components[i].UpdateGhostModelColor(Builder.canPlace, ref value);
                }
                ((Material)typeof(Builder).GetField("ghostStructureMaterial", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue( null)).SetColor(ShaderPropertyID._Tint, value);
                */

                return false; // return false to skip the original method 
            }
            return true; // if local player does something, return true to let original method execute
        }

        // On setting up the renderers, subnautica normally would evaluate if the player is currently inside or outside to apply 
        // the renderer for the view layer to the object that is currently created. On initial sync or remote action the visual 
        // ghost is never seen, so set to false.
        // ## TODO BUILDING ## maybe remove patch completely later
        internal bool Builder_SetupRenderers_Pre(ref bool interior)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_SetupRenderers_Pre");
#endif

                interior = false;
            }
            return true; // Let the original method execute with the changed inputparameter
        }

        // For objects outside on surfaces or objects inside on wall or ceiling surfaces (e.g. posters) only retrieve the target surface and perform no checks
        internal bool Builder_CheckAsSubModule_Pre(ref bool _result)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_CheckAsSubModule_Pre");
#endif

                if (!Constructable.CheckFlags((bool)typeof(Builder).GetField("allowedInBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null), (bool)typeof(Builder).GetField("allowedInSub", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null), (bool)typeof(Builder).GetField("allowedOutside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)))
                {
                    _result = false;
                    return false;
                }
                Transform aimTransform = Builder.GetAimTransform();
                typeof(Builder).GetField("placementTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, null);
                RaycastHit hit;


                // ## TODO BUILDING ## Doesn't work, because cyclops Ids are not stable! Reactivate the outcommented lower part when Cyclops ID is fixed and remove the exit lines below
                if (currentConstructedNewBasePiece.RotationMetadata.HasValue && currentConstructedNewBasePiece.RotationMetadata.Value is SubModuleRotationMetadata && currentConstructedNewBasePiece.ParentId.HasValue)
                {
                    _result = false;
                    return false;
                }
                /*
                if (currentConstructedNewBasePiece.RotationMetadata.HasValue && currentConstructedNewBasePiece.RotationMetadata.Value is SubModuleRotationMetadata && currentConstructedNewBasePiece.ParentId.HasValue)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Builder_CheckAsSubModule_Pre - check as cyclops child");

                    GameObject subRootGameObject = NitroxEntity.GetObjectFrom(currentConstructedNewBasePiece.ParentId.Value).OrElse(null);
                    if(subRootGameObject != null)
                    {
                        Log.Error("Builder_CheckAsSubModule_Pre - No GameObject with id: " + subRootGameObject);
                    }

                    SubRoot cyclops = subRootGameObject.GetComponent<SubRoot>();

                    NitroxModel.Logger.Log.Debug("Builder_CheckAsSubModule_Pre - subroot: " + cyclops );
                    if (cyclops != null)
                    {
                        NitroxModel.Logger.Log.Debug("Builder_CheckAsSubModule_Pre - isCyclops: " + cyclops.isCyclops);
                    }
#endif

                    if (subRootGameObject != null && subRootGameObject.GetComponent<SubRoot>()!= null && subRootGameObject.GetComponent<SubRoot>().isCyclops)
                    {
                        SubModuleRotationMetadata rotationMetadata = (SubModuleRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value;
                        Vector3 positionOffset = subRootGameObject.transform.position - rotationMetadata.ParentPosition.ToUnity();
                        Quaternion rotationOffset = subRootGameObject.transform.rotation * Quaternion.Inverse(rotationMetadata.ParentRotation.ToUnity());

                        Transform subAimTransform = new GameObject().transform;
                        subAimTransform.position = aimTransform.position + positionOffset;
                        subAimTransform.rotation = rotationOffset * aimTransform.rotation;

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("Builder_CheckAsSubModule_Pre - cameraPosition: " + subAimTransform.position + " cameraRotation: " + subAimTransform.rotation);
#endif

                        if (Physics.Raycast(subAimTransform.position, subAimTransform.forward, out hit, (float)typeof(Builder).GetField("placeMaxDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null), ((LayerMask)typeof(Builder).GetField("placeLayerMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).value, QueryTriggerInteraction.Ignore))
                        {
                            typeof(Builder).GetField("placementTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, hit.collider.gameObject);
                            _result = true;
                            return false;
                        }
                    }
                }*/

                if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, (float)typeof(Builder).GetField("placeMaxDistance", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null), ((LayerMask)typeof(Builder).GetField("placeLayerMask", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).value, QueryTriggerInteraction.Ignore))
                {
                    _result = false;
                    return false;
                }
                typeof(Builder).GetField("placementTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, hit.collider.gameObject);

                // skip the surface position calculation because it is already known

                // skipt the rest of the checks

                _result = true;
                return false;
            }
            return true;
        }

        internal bool Builder_TryPlace_Pre(ref bool _result)
        {
            if (remoteEventActive)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_TryPlace_Pre");
#endif

                typeof(Builder).GetMethod("Initialize", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, null);
                if (typeof(Builder).GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null) == null || !Builder.canPlace)
                {
                    _result = false;
                    return false;
                }

                //skip playing sound

                ConstructableBase componentInParent = ((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).GetComponentInParent<ConstructableBase>();
                if (componentInParent != null)
                {
                    BaseGhost component = ((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).GetComponent<BaseGhost>();
                    component.Place();
                    if (component.TargetBase != null)
                    {
                        componentInParent.transform.SetParent(component.TargetBase.transform, true);
                    }
                    componentInParent.SetState(false, true);

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("Builder_TryPlace_Pre - state set");

                    if(component is BaseAddModuleGhost)
                    {
                        NitroxModel.Logger.Log.Debug("Builder_TryPlace_Pre - ModuleFace: " + ((BaseAddModuleGhost)component).anchoredFace);
                    }
#endif

                    currentconstructedGameObject = componentInParent.gameObject;
                }
                else
                {
                    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>((GameObject)typeof(Builder).GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null));
                    bool flag = false;
                    bool flag2 = false;

                    // instead of setting the subroot by the current player location, use the saved parent
                    // orig: SubRoot currentSub = Player.main.GetCurrentSub();

                    SubRoot currentSub = null;
                    GameObject rootObject = NitroxEntity.GetObjectFrom(currentConstructedNewBasePiece.ParentId.Value).OrElse(null);

                    // set the subroot only if it is an inside object, outside objects will find the transform parent by placementTarget
                    if (rootObject != null && !(bool)typeof(Builder).GetField("allowedOutside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null))
                    {
                        currentSub = rootObject.GetComponent<SubRoot>();
                    }

                    if (currentSub != null)
                    {
                        flag = currentSub.isBase;
                        flag2 = currentSub.isCyclops;
                        gameObject.transform.parent = currentSub.GetModulesRoot();
                    }
                    else if (((GameObject)typeof(Builder).GetField("placementTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)) != null && ((bool)typeof(Builder).GetField("allowedOutside", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)))
                    {
                        SubRoot componentInParent2 = ((GameObject)typeof(Builder).GetField("placementTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)).GetComponentInParent<SubRoot>();
                        if (componentInParent2 != null)
                        {
                            gameObject.transform.parent = componentInParent2.GetModulesRoot();
                        }
                    }
                    Transform transform = gameObject.transform;
                    transform.position = (Vector3)typeof(Builder).GetField("placePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
                    transform.rotation = (Quaternion)typeof(Builder).GetField("placeRotation", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
                    Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
                    componentInParent3.SetState(false, true);
                    global::Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer(flag ? "Default" : "Interior"), true, -1);
                    if (((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)) != null)
                    {
                        UnityEngine.Object.Destroy(((GameObject)typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null)));
                    }
                    componentInParent3.SetIsInside(flag || flag2);
                    SkyEnvironmentChanged.Send(gameObject, currentSub);

                    currentconstructedGameObject = gameObject;
                }

                typeof(Builder).GetField("ghostModel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, null);
                typeof(Builder).GetField("prefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).SetValue(null, null);
                typeof(Builder).GetProperty("canPlace", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).SetValue(null, false, null);



                _result = true;
                return false;
            }
            return true;
        }

        #endregion

        #region Position and Metadata applyment to BaseGhosts

        // Apply the basic position of the builded Piece
        internal void Builder_SetDefaultPlaceTransform_Post(ref Vector3 position, ref Quaternion rotation)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_SetDefaultPlaceTransform_Post");
#endif

                position = currentConstructedNewBasePiece.ItemPosition.ToUnity();
                rotation = currentConstructedNewBasePiece.Rotation.ToUnity();
            }
        }

        // Apply camera position
        internal void Builder_GetAimTransform_Post(ref Transform result)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Builder_GetAimTransform_Post");
#endif
                result = new GameObject().transform;
                result.position = currentConstructedNewBasePiece.CameraPosition.ToUnity();
                result.rotation = currentConstructedNewBasePiece.CameraRotation.ToUnity();
            }
        }

        // Apply RotationMetaData to CorridorGhosts
        internal bool BaseAddCorridorGhost_UpdateRotation_Pre(BaseAddCorridorGhost instance, ref bool geometryChanged)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null && currentConstructedNewBasePiece.RotationMetadata.HasValue)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddCorridorGhost_UpdateRotation_Pre");
#endif

                // apply saved rotation data instead of BuilderTool input
                instance.ReflectionSet("rotation", ((BaseCorridorRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).Rotation);

                instance.ReflectionSet("corridorType", (int)instance.ReflectionCall("CalculateCorridorType"));
                ((Base)instance.ReflectionGet("ghostBase")).SetCorridor(Int3.zero, (int)instance.ReflectionGet("corridorType"), instance.isGlass);
                instance.ReflectionCall("RebuildGhostGeometry");
                geometryChanged = true;

                return false;
            }
            return true;
        }

        // Apply RotationMetaData to MapRoomGhosts
        internal bool BaseAddMapRoomGhost_UpdateRotation_Pre(BaseAddMapRoomGhost instance, ref bool geometryChanged)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null && currentConstructedNewBasePiece.RotationMetadata.HasValue)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddMapRoomGhost_UpdateRotation_Pre");
#endif

                // apply saved rotation data instead of BuilderTool input
                instance.ReflectionSet("cellType", (Base.CellType)((BaseMapRoomRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).CellType);
                instance.ReflectionSet("connectionMask", ((BaseMapRoomRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).ConnectionMask);

                ((Base)instance.ReflectionGet("ghostBase")).SetCell(Int3.zero, (Base.CellType)instance.ReflectionGet("cellType"));
                instance.ReflectionCall("RebuildGhostGeometry", null);
                geometryChanged = true;

                return false;
            }
            return true;
        }

        // Apply Position and RotationMetaData to ModuleGhosts
        internal bool BaseAddModuleGhost_UpdatePlacement_Pre(BaseAddModuleGhost instance, ref bool _result, Transform camera, float placeMaxDistance, ref bool positionFound, ref bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null && currentConstructedNewBasePiece.RotationMetadata.HasValue)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_UpdatePlacement_Pre");
#endif

                positionFound = false;
                geometryChanged = false;

                // apply saved rotation data instead of BuilderTool input
                instance.ReflectionSet("direction", ((BaseModuleRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).ModuleDirection);
                geometryChanged = true;

                // skip the check if Player is in base to also place modules at initialsync or by a remote player


                instance.ReflectionSet("targetBase", typeof(BaseGhost).GetMethod("FindBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { camera, 20f }));
                if (instance.ReflectionGet("targetBase") == null)
                {
                    geometryChanged = (bool)instance.ReflectionCall("SetupInvalid", null);
                    _result = false;
                    return false;
                }

                // skip face validation

                ((Base)instance.ReflectionGet("targetBase")).transform.InverseTransformDirection(camera.forward);
                Base.Face face = new Base.Face(((Base)instance.ReflectionGet("targetBase")).WorldToGrid(camera.position), (Base.Direction)instance.ReflectionGet("direction"));

                Int3 u = ((Base)instance.ReflectionGet("targetBase")).NormalizeCell(face.cell);
                face.cell = u + new Int3(1, 0, 1);

                // apply anchored face
                Int3 @int = ((Base)instance.ReflectionGet("targetBase")).NormalizeCell(face.cell);

                Base.Face face2 = new Base.Face(face.cell - ((Base)instance.ReflectionGet("targetBase")).GetAnchor(), face.direction);
                //Int3 baseAnchoredCell = moduleAnchoredFace.cell + ((Base)instance.ReflectionGet("targetBase")).GetAnchor();

                if (instance.anchoredFace == null || instance.anchoredFace.Value != face2)
                {

#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_UpdatePlacement_Pre - anchoredFace: " + instance.anchoredFace);
#endif

                    instance.anchoredFace = new Base.Face?(face2);


#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_UpdatePlacement_Pre - anchoredFaceValue: " + instance.anchoredFace.Value);
#endif


                    Base.CellType cell = ((Base)instance.ReflectionGet("targetBase")).GetCell(@int);
                    Int3 int2 = Base.CellSize[(int)cell];
                    geometryChanged = (bool)instance.ReflectionCall("UpdateSize", false, false, new object[] { int2 });
                    instance.GhostBase.CopyFrom(((Base)instance.ReflectionGet("targetBase")), new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
                    Int3 cell2 = face.cell - @int;
                    Base.Face face3 = new Base.Face(cell2, face.direction);
                    instance.GhostBase.SetFace(face3, instance.faceType);
                    instance.GhostBase.ClearMasks();
                    instance.GhostBase.SetFaceMask(face3, true);
                    instance.ReflectionCall("RebuildGhostGeometry", null);

                    instance.anchoredFace = new Base.Face?(face2);
#if TRACE && BUILDING
                    NitroxModel.Logger.Log.Debug("BaseAddModuleGhost_UpdatePlacement_Pre - anchoredFaceValue: " + instance.anchoredFace.Value);
#endif

                    geometryChanged = true;
                }

                ghostModelParentConstructableBase.transform.position = ((Base)instance.ReflectionGet("targetBase")).GridToWorld(@int);
                ghostModelParentConstructableBase.transform.rotation = ((Base)instance.ReflectionGet("targetBase")).transform.rotation;
                positionFound = true;

                _result = !((Base)instance.ReflectionGet("targetBase")).IsCellUnderConstruction(face.cell);

                return false;
            }
            return true;
        }


        // Apply Position and RotationMetaData to FaceGhosts
        internal bool BaseAddFaceGhost_UpdatePlacement_Pre(BaseAddFaceGhost instance, ref bool _result, Transform camera, float placeMaxDistance, ref bool positionFound, ref bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null && currentConstructedNewBasePiece.RotationMetadata.HasValue)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddFaceGhost_UpdatePlacement_Pre");
#endif

                positionFound = false;
                geometryChanged = false;

                instance.ReflectionSet("targetBase", typeof(BaseGhost).GetMethod("FindBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { camera, 20f }));
                if (instance.ReflectionGet("targetBase") == null)
                {
                    geometryChanged = (bool)instance.ReflectionCall("SetupInvalid", null);
                    _result = false;
                    return false;
                }

                // skip face validation

                // apply anchored face
                Base.Face moduleAnchoredFace = new Base.Face(((BaseFaceRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).AnchoredFaceCell.Global(), (Base.Direction)((BaseFaceRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).AnchoredFaceDirection);
                Int3 baseAnchoredCell = moduleAnchoredFace.cell + ((Base)instance.ReflectionGet("targetBase")).GetAnchor();
                Int3 @int = ((Base)instance.ReflectionGet("targetBase")).NormalizeCell(baseAnchoredCell);

                Base.CellType cell = ((Base)instance.ReflectionGet("targetBase")).GetCell(@int);
                Int3 v = Base.CellSize[(int)cell];
                Int3.Bounds a = new Int3.Bounds(baseAnchoredCell, baseAnchoredCell);
                Int3.Bounds b = new Int3.Bounds(@int, @int + v - 1);
                Int3.Bounds bounds = Int3.Bounds.Union(a, b);
                geometryChanged = (bool)instance.ReflectionCall("UpdateSize", false, false, new object[] { bounds.size });

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddFaceGhost_UpdatePlacement_Pre - setting anchoredFace");
#endif
                instance.anchoredFace = new Base.Face?(moduleAnchoredFace);

                instance.GhostBase.CopyFrom((Base)instance.ReflectionGet("targetBase"), bounds, bounds.mins * -1);
                instance.GhostBase.ClearMasks();
                Int3 cell2 = baseAnchoredCell - @int;
                Base.Face face3 = new Base.Face(cell2, (Base.Direction)((BaseFaceRotationMetadata)currentConstructedNewBasePiece.RotationMetadata.Value).AnchoredFaceDirection);
                instance.GhostBase.SetFaceMask(face3, true);
                instance.GhostBase.SetFace(face3, instance.faceType);

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddFaceGhost_UpdatePlacement_Pre - rebuilding ghost geometry");
#endif
                instance.ReflectionCall("RebuildGhostGeometry", null);
                geometryChanged = true;

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddFaceGhost_UpdatePlacement_Pre - setting position and rotation");
#endif

                ghostModelParentConstructableBase.transform.position = ((Base)instance.ReflectionGet("targetBase")).GridToWorld(@int);
                ghostModelParentConstructableBase.transform.rotation = ((Base)instance.ReflectionGet("targetBase")).transform.rotation;
                positionFound = true;

                foreach (Int3 cell3 in bounds)
                {
                    if (((Base)instance.ReflectionGet("targetBase")).IsCellUnderConstruction(cell3))
                    {

#if TRACE && BUILDING
                        NitroxModel.Logger.Log.Debug("BaseAddFaceGhost_UpdatePlacement_Pre - cell under construction");
#endif

                        _result = false;
                        return false;
                    }
                }

                _result = true;
                return false;
            }
            return true;
        }

        // Apply Position and RotationMetaData to Bulkheads
        internal bool BaseAddBulkheadGhost_UpdatePlacement_Pre(BaseAddBulkheadGhost instance, ref bool _result, Transform camera, float placeMaxDistance, ref bool positionFound, ref bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("BaseAddBulkheadGhost_UpdatePlacement_Pre");
#endif

                positionFound = false;
                geometryChanged = false;

                // skip the check if Player is in base to also place modules at initialsync or by a remote player

                instance.ReflectionSet("targetBase", typeof(BaseGhost).GetMethod("FindBase", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).Invoke(null, new object[] { camera, 20f }));
                if (instance.ReflectionGet("targetBase") == null)
                {
                    geometryChanged = (bool)instance.ReflectionCall("SetupInvalid", null);
                    _result = false;
                    return false;
                }

                // find the face by camera because it didn't got saved in rotationmetadata
                Vector3 normal = ((Base)instance.ReflectionGet("targetBase")).transform.InverseTransformDirection(camera.forward);
                Base.Face adjacentFace = new Base.Face(((Base)instance.ReflectionGet("targetBase")).WorldToGrid(camera.position), Base.NormalToDirection(normal));

                // skip face validation

                Int3 @int = ((Base)instance.ReflectionGet("targetBase")).NormalizeCell(adjacentFace.cell);

                Base.CellType cell = ((Base)instance.ReflectionGet("targetBase")).GetCell(@int);
                Int3 int2 = Base.CellSize[(int)cell];
                if (instance.GhostBase.Shape.ToInt3() != int2)
                {
                    instance.GhostBase.SetSize(int2);
                    instance.GhostBase.AllocateMasks();
                }
                instance.GhostBase.CopyFrom(((Base)instance.ReflectionGet("targetBase")), new Int3.Bounds(@int, @int + int2 - 1), @int * -1);
                Int3 cell2 = adjacentFace.cell - @int;
                Base.Face face = new Base.Face(cell2, adjacentFace.direction);
                instance.GhostBase.SetFace(face, Base.FaceType.BulkheadOpened); //original is closed, let it open as long as doors are not synced
                instance.GhostBase.ClearMasks();
                instance.GhostBase.SetFaceMask(face, true);
                instance.ReflectionCall("RebuildGhostGeometry", null);
                geometryChanged = true;
                instance.ReflectionSet("face", new Base.Face?(adjacentFace));

                ghostModelParentConstructableBase.transform.position = ((Base)instance.ReflectionGet("targetBase")).GridToWorld(@int);
                ghostModelParentConstructableBase.transform.rotation = ((Base)instance.ReflectionGet("targetBase")).transform.rotation;
                positionFound = true;

                if (((Base)instance.ReflectionGet("targetBase")).IsCellUnderConstruction(adjacentFace.cell) || (((Base)instance.ReflectionGet("targetBase")).IsCellUnderConstruction(Base.GetAdjacent(adjacentFace))))
                {
                    _result = false;
                    return false;
                }

                _result = true;
                return false;
            }
            return true;
        }

        // At placing the first BasePiece use the fix saved position
        internal void BaseGhost_PlaceWithBoundsCast_Post(BaseGhost instance, ref bool _result, ref Vector3 center)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {
                center = currentConstructedNewBasePiece.ItemPosition.ToUnity();
                _result = true;
            }
        }

        // Return true to skip any further checks which are only needed if the local player 
        // creates a new object
        internal bool Constructable_CheckFlags_Pre(ref bool _result)
        {
            if (remoteEventActive && currentConstructedNewBasePiece != null)
            {
                _result = true;
                return false;
            }
            return true;
        }

        #endregion
    }
}
