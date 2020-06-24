using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
using NitroxClient.MonoBehaviours.Overrides;
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

namespace NitroxClient.GameLogic
{
    public class Building 
    {
        // State if currently in InitialSync phase
        public bool isInitialSyncing = false;

        // Contains the last hovered constructable object hovered by the current player. Is needed to ensure fired events for the correct item.
        public Constructable lastHoveredConstructable = null;

        // State of currently using BuilderTool or not
        public bool currentlyHandlingBuilderTool = false;

        // State if a construction Event is raised by Initialsync or a current remote player.
        public bool remoteEventActive = false;

        // For the base objects themself as master objects of a base-complex we can't assign Ids to the ghosts, 
        public Dictionary<GameObject, NitroxId> baseGhostsIDCache = new Dictionary<GameObject, NitroxId>();

        public NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.SubnauticaRotationMetadataFactory rotationMetadataFactory = new NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.SubnauticaRotationMetadataFactory();

        public bool InitialSyncActive { set => isInitialSyncing = value; }

        public void ConstructNewBasePiece(BasePiece basePiece)
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

        public void ChangeConstructAmount(NitroxId id, float constructionAmount)
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

        public void FinishConstruction(NitroxId id)
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

        public void DeconstructBasePiece(NitroxId id)
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

        public void FinishDeconstruction(NitroxId id)
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
       
    }
}
