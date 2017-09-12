using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class Player_Awake_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Player);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Awake", BindingFlags.Public | BindingFlags.Instance);
        
        public static void Postfix()
        {
            NitroxServer.Server server = Player.main.GetComponent<NitroxServer.Server>();

            if(server == null)
            {
                Player.main.gameObject.AddComponent<NitroxServer.Server>();
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
