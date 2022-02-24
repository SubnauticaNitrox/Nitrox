using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public class uGUI_PDA_SetTabs_Patch : NitroxPatch, IDynamicPatch
{
    private readonly static MethodInfo TARGET_METHOD = Reflect.Method((uGUI_PDA t) => t.SetTabs(default));

    public static bool Prefix(uGUI_PDA __instance, List<PDATab> tabs)
    {
        int num = (tabs != null) ? tabs.Count : 0;
        Atlas.Sprite[] array = new Atlas.Sprite[num];
        __instance.currentTabs.Clear();
        for (int i = 0; i < num; i++)
        {
            PDATab item = tabs[i];
            array[i] = SpriteManager.Get(SpriteManager.Group.Tab, string.Format("Tab{0}", item.ToString()));
            
            __instance.currentTabs.Add(item);
        }
        // the last tab is the one we added in uGUI_PDA_Initialize_Patch
        if (AssetsHelper.AssetBundleLoaded)
        {
            Atlas.Sprite oldSprite = array[num - 1];
            Log.Debug($"Old sprite: pixelsPerUnit: {oldSprite.pixelsPerUnit}, padding: {oldSprite.padding}, size: {oldSprite.size}");
            // array[num - 1].texture = AssetsHelper.GetTexture("player_list_tab@2x");
            array[num - 1] = AssetsHelper.MakeAtlasSpriteFromTexture("player_list_tab@4x");
        }
        else
        {
            // As a placeholder, we use the normal player icon
            array[num - 1] = array[0];
            AssetsHelper.onPlayerListAssetsLoaded += () => { AssignSprite(__instance.toolbar); };
        }

        uGUI_Toolbar uGUI_Toolbar = __instance.toolbar;
        object[] content = array;
        uGUI_Toolbar.Initialize(__instance, content, null, 15);
        __instance.CacheToolbarTooltips();
        return false;
    }

    private static void AssignSprite(uGUI_Toolbar uGUI_Toolbar)
    {
        // Last is player list tab's one
        uGUI_Toolbar.icons.GetLast().SetForegroundSprite(AssetsHelper.MakeAtlasSpriteFromTexture("player_list_tab@2x"));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
