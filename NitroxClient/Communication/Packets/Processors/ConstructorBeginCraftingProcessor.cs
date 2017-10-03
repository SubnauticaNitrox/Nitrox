using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Helper.Unity;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static NitroxModel.Helper.GameLogic.TransientLocalObjectManager;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructorBeginCraftingProcessor : ClientPacketProcessor<ConstructorBeginCrafting>
    {
        public static GameObject ConstructedObject;

        public override void Process(ConstructorBeginCrafting packet)
        {
            GameObject gameObject = GuidHelper.RequireObjectFrom(packet.ConstructorGuid);
            Crafter crafter = gameObject.RequireComponentInChildren<Crafter>(true);
                                    
            MethodInfo onCraftingBegin = typeof(Crafter).GetMethod("OnCraftingBegin", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(onCraftingBegin);
            onCraftingBegin.Invoke(crafter, new object[] { packet.TechType, packet.Duration }); //TODO: take into account latency for duration   

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if(opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();
                GuidHelper.SetNewGuid(constructedObject, packet.ConstructedItemGuid);

                SetInteractiveChildrenGuids(constructedObject, packet.InteractiveChildIdentifiers);
            }
            else
            {
                Log.Error("Could not find constructed object!");
            }
        }

        private void SetInteractiveChildrenGuids(GameObject constructedObject, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers)
        {
            foreach(InteractiveChildObjectIdentifier childIdentifier in interactiveChildIdentifiers)
            {
                UnityEngine.Transform transform = constructedObject.transform.Find(childIdentifier.GameObjectNamePath);

                if(transform != null)
                {
                    GameObject gameObject = transform.gameObject;
                    GuidHelper.SetNewGuid(gameObject, childIdentifier.Guid);
                }
                else
                {
                    Log.Error("Error GUID tagging interactive child due to not finding it: " + childIdentifier.GameObjectNamePath);
                }
            }
        }
    }
}
