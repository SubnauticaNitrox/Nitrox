using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
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

            NitroxId id = NitroxEntity.GetId(constructableBase.gameObject);
            NitroxId parentBaseId = (targetBase == null) ? null : NitroxEntity.GetId(targetBase.gameObject);
            Vector3 placedPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

            BasePiece basePiece = new BasePiece(id, placedPosition, quaternion, camera.position, camera.rotation, techType.Model(), Optional<NitroxId>.OfNullable(parentBaseId), false, rotationMetadata);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            packetSender.Send(placedBasePiece);
        }

        public void PlaceFurniture(GameObject gameObject, TechType techType, Vector3 itemPosition, Quaternion quaternion)
        {
            if (!Builder.isPlacing) //prevent possible echoing
            {
                return;
            }

            NitroxId id = NitroxEntity.GetId(gameObject);

            Optional<NitroxId> subId = Optional<NitroxId>.Empty();
            SubRoot sub = Player.main.currentSub;
            if (sub != null)
            {
                subId = Optional<NitroxId>.Of(NitroxEntity.GetId(sub.gameObject));
            }

            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = Optional<RotationMetadata>.Empty();

            BasePiece basePiece = new BasePiece(id, itemPosition, quaternion, camera.position, camera.rotation, techType.Model(), subId, true, rotationMetadata);
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
            
            NitroxId id = NitroxEntity.GetId(gameObject);

            if (amount < 0.95f) // Construction complete event handled by function below
            {
                ConstructionAmountChanged amountChanged = new ConstructionAmountChanged(id, amount);
                packetSender.Send(amountChanged);
            }
        }

        public void ConstructionComplete(GameObject ghost)
        {
            NitroxId baseId = null;
            Optional<object> opConstructedBase = TransientLocalObjectManager.Get(TransientObjectType.BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT);

            NitroxId id = NitroxEntity.GetId(ghost);

            if (opConstructedBase.IsPresent())
            {
                GameObject constructedBase = (GameObject)opConstructedBase.Get();
                baseId = NitroxEntity.GetId(constructedBase);
            }
            
            // For base pieces, we must switch the id from the ghost to the newly constructed piece.
            // Furniture just uses the same game object as the ghost for the final product.
            if(ghost.GetComponent<ConstructableBase>() != null)
            {
                Optional<object> opBasePiece = TransientLocalObjectManager.Get(TransientObjectType.LATEST_CONSTRUCTED_BASE_PIECE);
                GameObject finishedPiece = (GameObject)opBasePiece.Get();
                
                UnityEngine.Object.Destroy(ghost);
                NitroxEntity.SetNewId(finishedPiece, id);

                if(baseId == null)
                {
                    baseId = NitroxEntity.GetId(finishedPiece.GetComponentInParent<Base>().gameObject);
                }
            }

            ConstructionCompleted constructionCompleted = new ConstructionCompleted(id, baseId);
            packetSender.Send(constructionCompleted);
        }

        public void DeconstructionBegin(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);

            DeconstructionBegin deconstructionBegin = new DeconstructionBegin(id);
            packetSender.Send(deconstructionBegin);
        }

        public void DeconstructionComplete(GameObject gameObject)
        {
            NitroxId id = NitroxEntity.GetId(gameObject);

            DeconstructionCompleted deconstructionCompleted = new DeconstructionCompleted(id);
            packetSender.Send(deconstructionCompleted);
        }
    }
}
