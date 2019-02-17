using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers.Abstract;
using UWE;

namespace NitroxServer_Subnautica.Serialization.ResourceAssets.MonoBehaviourParsers
{
    class WorldEntityDataParser : MonoBehaviourParser
    {
        public override void Parse(AssetsFileInstance instance, long gameObjectPathId, ResourceAssets resourceAssets)
        {
            AssetsFile resourcesFile = instance.file;
            resourcesFile.reader.Align();
            uint size = resourcesFile.reader.ReadUInt32();
            WorldEntityInfo wei;
            for (int i = 0; i < size; i++)
            {
                wei = new WorldEntityInfo();
                wei.classId = resourcesFile.reader.ReadCountStringInt32();
                wei.techType = (TechType)resourcesFile.reader.ReadInt32();
                wei.slotType = (EntitySlot.Type)resourcesFile.reader.ReadInt32();
                wei.prefabZUp = resourcesFile.reader.ReadBoolean();
                resourcesFile.reader.Align();
                wei.cellLevel = (LargeWorldEntity.CellLevel)resourcesFile.reader.ReadInt32();
                wei.localScale = new UnityEngine.Vector3(resourcesFile.reader.ReadSingle(), resourcesFile.reader.ReadSingle(), resourcesFile.reader.ReadSingle());
                resourceAssets.WorldEntitiesByClassId.Add(wei.classId, wei);
            }
        }
    }
}
