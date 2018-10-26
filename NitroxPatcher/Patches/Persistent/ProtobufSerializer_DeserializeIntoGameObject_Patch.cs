using System;
using System.Reflection;
using Harmony;
using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using UnityEngine;

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
            "61a5e0e6-01d5-4ae2-aea6-1186cd769025", // Coral_reef_purple_mushrooms_01_01
            "fc7c1098-13af-417a-8038-0053b65498e5", // Coral_reef_purple_mushrooms_01_02 
            "31834aae-35ce-49c1-b5ba-ac4227750679", // Coral_reef_purple_mushrooms_01_03 
            "99cdec62-302b-4999-ba49-f50c73575a4d", // Coral_reef_purple_mushrooms_01_04 
            "f78942c3-87e7-4015-865a-5ae4d8bd9dcb", // Reaper
            "061af756-643c-42ad-9645-a522f1338084", // Coral_reef_slanted_coral_plates_01_01
            "70eb6270-bf5e-4d6a-8182-484ffcfd8de6", // Coral_reef_jeweled_disk_red_01_01
            "4e31161e-c812-4c8c-bfd4-00cf4b743884", // Coral_reef_jeweled_disk_red_01_04
            "f0713f3d-586b-4c71-88a3-18dd6c3dd2a4", // Coral_reef_jeweled_disk_red_01_02
            "e8047056-e202-49b3-829f-7458615103ac", // Coral_reef_purple_tentacle_plant_01_01
            "3dbab1b9-cc52-4da4-8633-89b33add18f4", // Coral_reef_purple_tentacle_plant_01_02
            "22bf7b03-8154-410b-a6fb-8ba315f68987", // coral_reef_small_deco_15_red
            "3eba5a45-624a-4d5b-9a4b-0c3bad357a96", // Coral_Clump02c
            "0063d51a-de77-438b-a592-61f68d12f4ad", // Coral_Clump02b
            "e80b22ff-064d-46ca-b71e-456d6b3426ab", // Coral_reef_purple_fan
            "9a9cdb4e-f110-412d-b16b-b9ace904b569", // FloatingStone5_Floaters
            "7ecc9cdd-3afc-4005-bff7-01ba62e95a03", // coral_reef_small_deco_12
            "06562999-e575-4b02-b880-71d37616b5b9", // Coral_reef_shell_tunnel_01
            "93a9886d-f2d3-4b6c-8e5f-216f569f82b2", // Coral_reef_slanted_coral_plates_01_02
            "691723cf-d5e9-482f-b5af-8491b2a318b1", // Coral_reef_shell_tunnel_03
            "171c6a5b-879b-4785-be7a-6584b2c8c442", // BrainCoral
            "a1f3da68-d810-44ff-a0a2-6cf3c6a3eff5", // FloatingStone5
            "f0295655-8f4f-4b18-b67d-925982a472d7", // Coral_reef_shell_tunnel_02
            "2d970c98-6f77-4270-8be2-91dc863d15d5", // Coral_reef_jeweled_disk_purple_01_01
            "df03263c-ebfb-4e7c-b002-1ec3d67c1215", // Coral_reef_jeweled_disk_purple_01_03
            "eb6634e5-3a58-4a0d-ae4e-b673e1fa51ea", // Coral_reef_jeweled_disk_purple_01_02
            "c197a6ca-f910-43db-92ab-2e35e423a6f1", // Coral_reef_jeweled_disk_purple_01_04
            "9a643563-9278-4c77-8bd2-f9b4b1a1053a", // Coral_reef_jeweled_disk_red_01_03
            "37ea521a-6be4-437c-8ed7-6b453d9218a8", // FloaterLarge
            "e4f6fd0f-82ec-49ee-8546-6b770d118f61", // Fern 01
            "e205ce39-5e36-4c21-b981-b6240da26449", // Fern 02
            "a4be67bb-f6e1-4d15-bf08-9d9a3fae4bfa", // Tropical Plant 1a
            "28ec1137-da13-44f3-b76d-bac12ab766d1", // land_plant_small_01_01
            "98be0944-e0b3-4fba-8f08-ca5d322c22f6", // land_plant_small_02_02
            "abe4426a-5968-40b0-9d99-b06207984aa8", // Jungle Tree 3a
            "98b3ffc5-5497-49ad-8155-3608826ad373", // Jungle Tree 3b
            "1cc51be0-8ea9-4730-936f-23b562a9256f", // Land_tree_01
            "0e394d55-da8c-4b3e-b038-979477ce77c1", // AbandonedBaseFloatingIsland3
            "7ce2ca9d-6154-4988-9b02-38f670e741b8", // CaveCrawler_02
            "c2b5b625-bd5b-4390-be53-2d9f3ef58f36", // IslandsPDARendezvous
            "12a7577b-68c4-45ae-8172-42abf649501e", // IslandsPDAExterior
            "5f93851f-cb13-4b99-99a9-15c34c063e13", // IslandsPDABase1a
            "99b164ac-dfb4-4a14-b305-8666fa227717", // AbandonedBaseFloatingIsland1
            "569f22e0-274d-49b0-ae5e-21ef0ce907ca", // AbandonedBaseFloatingIsland2
        };

        public static void Postfix(ProtobufSerializer.GameObjectData goData, UniqueIdentifier uid)
        {
            if (blacklistedHandPlacedClassIds.Contains(goData.ClassId) && Multiplayer.Main != null && Multiplayer.Main.IsMultiplayer())
            {
                UnityEngine.Object.Destroy(uid.gameObject);
            }
            else
            {
                MonoBehaviour.print(goData + " : " + uid);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
