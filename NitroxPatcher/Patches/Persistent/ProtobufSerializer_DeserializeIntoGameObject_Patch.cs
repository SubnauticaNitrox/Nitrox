using System;
using System.Reflection;
using Harmony;
using System.Collections.Generic;

// TODO: Temporarily persistent to run before everything.  When we migrate the patch hook to an early point then make this non-persistent
namespace NitroxPatcher.Patches.Persistent
{
    class ProtobufSerializer_DeserializeIntoGameObject_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(ProtobufSerializer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("DeserializeIntoGameObject", BindingFlags.NonPublic | BindingFlags.Instance);

        /*
         * We allow the client to spawn a lot of the hand placed items such as
         * wrecks, corals, debris, and other static things.  We want some hand-placed 
         * items to be only handled by the server; thus, we are blacklisting here.
         * Some day if we want all items to be authoritative by the server we can
         * parse Build18/cellCache/.
         */
        public static List<string> blacklistedHandPlacedClassIds = new List<string>()
        {
            "61a5e0e6-01d5-4ae2-aea6-1186cd769025", //Coral_reef_purple_mushrooms_01_01
            "fc7c1098-13af-417a-8038-0053b65498e5", // Coral_reef_purple_mushrooms_01_02 
            "31834aae-35ce-49c1-b5ba-ac4227750679", // Coral_reef_purple_mushrooms_01_03 
            "99cdec62-302b-4999-ba49-f50c73575a4d", // Coral_reef_purple_mushrooms_01_04 
            "f78942c3-87e7-4015-865a-5ae4d8bd9dcb" // Reaper
        };

        public static bool Prefix(ProtobufSerializer.GameObjectData goData)
        {
            if(blacklistedHandPlacedClassIds.Contains(goData.ClassId))
            {
                return false;
            }

            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
