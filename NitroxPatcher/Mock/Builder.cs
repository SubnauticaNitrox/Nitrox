using NitroxClient.MonoBehaviours;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxPatcher.Mock
{
    public class Builder
    {
        public static GameObject prefab;
        public static GameObject ghostModel;
        public static Quaternion placeRotation;

        public void code()
        {
            if (Multiplayer.isActive)
            {
                TechType techType = CraftData.GetTechType(Builder.prefab);
                Multiplayer.PacketSender.BuildItem(Enum.GetName(typeof(TechType), techType), Builder.ghostModel.transform.position, Builder.placeRotation);
            }
        }
    }
}
