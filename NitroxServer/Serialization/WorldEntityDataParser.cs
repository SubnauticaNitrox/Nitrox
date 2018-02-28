using System;
using System.Collections.Generic;
using System.IO;
using UWE;

namespace NitroxServer.Serialization
{
    public class WorldEntityDataParser
    {
        /**
         * Hacky implementation that parses the user-exported version of the WorldEntityData
         * resource (seen in UWE code via WorldEntityDatabase).  This file is primarily used
         * to surface the tuple classId, techType, and slotType.... these elements are later
         * used to join on loot distribution data to figure out where entities spawn.  We will
         * eventually want to replace this implementation with something that can automatically
         * mine this data from the subnautica resources.assets file.
         */
        public Dictionary<string, WorldEntityInfo> GetWorldEntitiesByClassId()
        {
            Dictionary<string, WorldEntityInfo> worldEntitiesByClassId = new Dictionary<string, WorldEntityInfo>();
            string file = Path.GetFullPath(@"..\..\..\raw\worlddata.txt");

            using (StreamReader reader = File.OpenText(file))
            {
                string line;
                int counter = 0;
                WorldEntityInfo nextEntityInfo = null;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] items = line.Split(' ');

                    switch (counter)
                    {
                        case 0:
                            nextEntityInfo = new WorldEntityInfo();
                            break;
                        case 1:
                            nextEntityInfo.classId = items[1];
                            worldEntitiesByClassId[nextEntityInfo.classId] = nextEntityInfo;
                            break;
                        case 2:
                            nextEntityInfo.techType = (TechType)Enum.Parse(typeof(TechType), items[1]);
                            break;
                        case 3:
                            nextEntityInfo.slotType = (EntitySlot.Type)Enum.Parse(typeof(EntitySlot.Type), items[1]);
                            break;
                        case 4:
                            nextEntityInfo.prefabZUp = bool.Parse(items[1]);
                            break;
                        case 5:
                            nextEntityInfo.cellLevel = (LargeWorldEntity.CellLevel)Enum.Parse(typeof(LargeWorldEntity.CellLevel), items[1]);
                            break;
                        case 6:
                            string[] dimensions = items[1].Split('-');
                            nextEntityInfo.localScale = new UnityEngine.Vector3(float.Parse(dimensions[0]), float.Parse(dimensions[1]), float.Parse(dimensions[2]));
                            break;
                    }

                    counter++;

                    if (counter == 7)
                    {
                        counter = 0;
                    }
                }
            }

            return worldEntitiesByClassId;
        }
    }
}
