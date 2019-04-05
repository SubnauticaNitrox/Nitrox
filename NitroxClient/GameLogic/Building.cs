using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class Building
    {
        private const float CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS = 0.10f;

        private readonly IPacketSender packetSender;
        private readonly RotationMetadataFactory rotationMetadataFactory;

        private float timeSinceLastConstructionChangeEvent;

        public Building(IPacketSender packetSender, RotationMetadataFactory rotationMetadataFactory)
        {
            this.packetSender = packetSender;
            this.rotationMetadataFactory = rotationMetadataFactory;
        }

        public void PlaceBasePiece(BaseGhost baseGhost, ConstructableBase constructableBase, Base targetBase, TechType techType, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            string guid = GuidHelper.GetGuid(constructableBase.gameObject);
            string parentBaseGuid = (targetBase == null) ? null : GuidHelper.GetGuid(targetBase.gameObject);
            Vector3 placedPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

            BasePiece basePiece = new BasePiece(guid, placedPosition, quaternion, camera.position, camera.rotation, techType.Model(), Optional<string>.OfNullable(parentBaseGuid), false, rotationMetadata);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            string guid = GuidHelper.GetGuid(gameObject);

            Optional<string> subGuid = Optional<string>.Empty();
            SubRoot sub = Player.main.currentSub;
            if (sub != null)
            {
                subGuid = Optional<string>.Of(GuidHelper.GetGuid(sub.gameObject));
            }

            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = Optional<RotationMetadata>.Empty();

            BasePiece basePiece = new BasePiece(guid, itemPosition, quaternion, camera.position, camera.rotation, techType.Model(), subGuid, true, rotationMetadata);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void ChangeConstructionAmount(GameObject gameObject, float amount)
        {
            timeSinceLastConstructionChangeEvent += Time.deltaTime;

            if (timeSinceLastConstructionChangeEvent < CONSTRUCTION_CHANGE_EVENT_COOLDOWN_PERIOD_SECONDS)
            {
                return;
            }

            timeSinceLastConstructionChangeEvent = 0.0f;
            
            string guid = GuidHelper.GetGuid(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(guid, amount);
                packetSender.Send(amountChanged);
            }
        }

        public void ConstructionComplete(GameObject ghost)
        {
            string baseGuid = null;
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            string guid = GuidHelper.GetGuid(ghost);

            if (opConstructedBase.IsPresent())
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Get();
                baseGuid = GuidHelper.GetGuid(constructedBase);
            }
            
            // For base pieces, we must switch the guid from the ghost to the newly constructed piece.
            // Furniture just uses the same game object as the ghost for the final product.
            if(ghost.GetComponent<ConstructableBase>() != null)
            {
                Optional<object> opBasePiece = TransientLocalObjectManager.Get(TransientObjectType.LATEST_CONSTRUCTED_BASE_PIECE);
                GameObject finishedPiece = (GameObject)opBasePiece.Get();
                
                UnityEngine.Object.Destroy(ghost);
                GuidHelper.SetNewGuid(finishedPiece, guid);

                if(baseGuid == null)
                {
                    baseGuid = GuidHelper.GetGuid(finishedPiece.GetComponentInParent<Base>().gameObject);
                }
            }

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(guid, baseGuid);
            packetSender.Send(constructionCompleted);
        }

        public void DeconstructionBegin(GameObject gameObject)
        {
            string guid = GuidHelper.GetGuid(gameObject);

            DeconstructionBegin deconstructionBegin = new DeconstructionBegin(guid);
            packetSender.Send(deconstructionBegin);
        }

        public void DeconstructionComplete(GameObject gameObject)
        {
            string guid = GuidHelper.GetGuid(gameObject);

            DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(guid);
            packetSender.Send(deconstructionCompleted);
        }




        private Constructable lastHoveredConstructable = null;

        //only triggered here for Fabricator and Workbench
        public void BuilderTool_Post_OnHover(GameObject gameObject, Constructable constructable)
        {

#if TRACE && GAMEEVENTBUILDING
            NitroxModel.Logger.Log.Debug("BuilderTool_Post_OnHover");
#endif

            lastHoveredConstructable = constructable;
            string _crafterGuid = GuidHelper.GetGuid(constructable.gameObject);
            ushort _remotePlayerId;
            if (NitroxServiceLocator.LocateService<SimulationOwnership>().HasExclusiveLockByRemotePlayer(_crafterGuid, out _remotePlayerId))
            {

#if TRACE && GAMEEVENTBUILDING
                    NitroxModel.Logger.Log.Debug("BuilderTool_Post_OnHover - lockedRemote guid: " + _crafterGuid + " remotePlayerID: " + _remotePlayerId);
#endif

                RemotePlayer _remotePlayer;
                if (NitroxServiceLocator.LocateService<PlayerManager>().TryFind(_remotePlayerId, out _remotePlayer))
                {
                    string _baseString = string.Empty;
                    if (constructable.constructed)
                    {
                        _baseString = NitroxServiceLocator.LocateService<TranslationManager>().GetTranslation("txtInUseBy {0}");
                    }
                    else
                    {
                        _baseString = NitroxServiceLocator.LocateService<TranslationManager>().GetTranslation("txtInConstructionBy {0}");
                    }
                    string _displayText =  string.Format(_baseString, _remotePlayer.PlayerName);
                    HandReticle.main.SetInteractText(_displayText, string.Empty);
#pragma warning disable 0618 // deprecated code here
                    //HandReticle.main.SetInteractInfo(_displayText, string.Empty); //<< must be used, otherwise it is not displayed > bug SN ? 
#pragma warning restore
                    HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
                }
                else
                {
                    //exclusive lock is active, but remote Player can not be found
                    //possible causes:
                    //- disconnect of remote Player did not free the lock locally
                    lastHoveredConstructable = null;
                    throw new System.Exception("BuilderTool_Post_OnHover: " + "No remote Player could be found for ID: " + _remotePlayerId);
                }
            }
            else
            {
                lastHoveredConstructable = null;
            }

        }

        public bool BuilderTool_Pre_HandleInput(GameObject gameObject)
        {

#if TRACE && GAMEEVENTBUILDING
            NitroxModel.Logger.Log.Debug("BuilderTool_Pre_HandleInput");
#endif

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
            }
        }
    }
}
