using System.Collections.Generic;
using AssetsTools.NET;
using Nitrox.Server.GameLogic.Entities;
using Nitrox.Server.Serialization.Resources.Datastructures;

namespace Nitrox.Server.Subnautica.Serialization.Resources.Parsers.Monobehaviours
{
    public class EntitySlotParser : MonobehaviourParser
    {
        public static Dictionary<AssetIdentifier, NitroxEntitySlot> EntitySlotsByIdentifier = new Dictionary<AssetIdentifier, NitroxEntitySlot>();

        public override void Parse(AssetIdentifier identifier, AssetIdentifier gameObjectIdentifier, AssetsFileReader reader, ResourceAssets resourceAssets)
        {
            int count = reader.ReadInt32(); // Array Count
            string[] allowedTypes = new string[count];

            for (int i = 0; i < count; i++)
            {
                allowedTypes[i] = ((EntitySlot.Type)reader.ReadInt32()).ToString();
            }

            string biomeType = ((BiomeType)reader.ReadInt32()).AsString(); // Yes

            EntitySlotsByIdentifier.Add(gameObjectIdentifier, new NitroxEntitySlot(biomeType, allowedTypes));
        }
    }
}
