using NitroxClient.Communication;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class MobileVehicleBay
    {
        private PacketSender packetSender;

        private List<Type> interactiveChildTypes = new List<Type>() // we must sync guids of these types when creating vehicles (mainly cyclops)
        {
            { typeof(Openable) },
            { typeof(CyclopsLocker) },
            { typeof(Fabricator) },
            { typeof(FireExtinguisherHolder) }
        };

        public MobileVehicleBay(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }
        
        public void BeginCrafting(GameObject constructor, TechType techType, float duration)
        {
            String constructorGuid = GuidHelper.GetGuid(constructor);

            Console.WriteLine("Building item from constructor with uuid: " + constructorGuid);

            Optional<object> opConstructedObject = TransientLocalObjectManager.Get(TransientObjectType.CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT);

            if (opConstructedObject.IsPresent())
            {
                GameObject constructedObject = (GameObject)opConstructedObject.Get();

                List<InteractiveChildObjectIdentifier> childIdentifiers = ExtractGuidsOfInteractiveChildren(constructedObject);
                String constructedObjectGuid = GuidHelper.GetGuid(constructedObject);

                ConstructorBeginCrafting beginCrafting = new ConstructorBeginCrafting(packetSender.PlayerId, constructorGuid, constructedObjectGuid, ApiHelper.TechType(techType), duration, childIdentifiers);
                packetSender.Send(beginCrafting);
            }
            else
            {
                Console.WriteLine("Could not send packet because there wasn't a corresponding constructed object!");
            }
        }

        private List<InteractiveChildObjectIdentifier> ExtractGuidsOfInteractiveChildren(GameObject constructedObject)
        {
            List<InteractiveChildObjectIdentifier> ids = new List<InteractiveChildObjectIdentifier>();

            String constructedObjectsName = constructedObject.GetFullName() + "/";

            foreach (Type type in interactiveChildTypes)
            {
                Component[] components = constructedObject.GetComponentsInChildren(type, true);

                foreach (Component component in components)
                {
                    String guid = GuidHelper.GetGuid(component.gameObject);
                    String componentName = component.gameObject.GetFullName();
                    String relativePathName = componentName.Replace(constructedObjectsName, "");

                    ids.Add(new InteractiveChildObjectIdentifier(guid, relativePathName));
                }
            }

            return ids;
        }
    }
}
