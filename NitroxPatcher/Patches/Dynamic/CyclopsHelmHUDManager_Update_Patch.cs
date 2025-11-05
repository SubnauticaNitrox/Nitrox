using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsHelmHUDManager_Update_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHelmHUDManager t) => t.Update());

    public static void Postfix(CyclopsHelmHUDManager __instance)
    {
        // To show the Cyclops HUD every time "hudActive" have to be true. "hornObject" is a good indicator to check if the player piloting the cyclops.
        if (!__instance.hornObject.activeSelf && __instance.hudActive)
        {
            __instance.canvasGroup.interactable = false;
        }
        else if (!__instance.hudActive)
        {
            __instance.hudActive = true;
        }

        if (__instance.subLiveMixin.IsAlive())
        {
            if (__instance.motorMode.engineOn)
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOn");
            }
            else
            {
                __instance.engineToggleAnimator.SetTrigger("EngineOff");
            }
        }

        // FIX: prevent negative/NaN power % when no cells are installed
        try
        {
            FieldInfo subRootFi = AccessTools.Field(typeof(CyclopsHelmHUDManager), "subRoot");
            SubRoot subRoot = null;
            if (subRootFi != null)
            {
                object value = subRootFi.GetValue(__instance);
                subRoot = value as SubRoot;
            }
            if (subRoot == null)
            {
                return;
            }

            PowerRelay relay = subRoot.powerRelay;
            if (relay == null)
            {
                return;
            }

            float current = relay.GetPower();
            float max = relay.GetMaxPower();
            float ratio = (max <= 0f) ? 0f : UnityEngine.Mathf.Clamp01(current / max);

            // Update percent text
            FieldInfo powerTextFi = AccessTools.Field(typeof(CyclopsHelmHUDManager), "powerText");
            TMPro.TextMeshProUGUI powerText = powerTextFi != null
                ? powerTextFi.GetValue(__instance) as TMPro.TextMeshProUGUI
                : null;

            if (powerText != null)
            {
                int percent = UnityEngine.Mathf.RoundToInt(ratio * 100f);
                powerText.text = $"{percent}%";
            }

            // Update bar (Slider or Image)
            FieldInfo powerBarFi = AccessTools.Field(typeof(CyclopsHelmHUDManager), "powerBar");
            if (powerBarFi != null)
            {
                object bar = powerBarFi.GetValue(__instance);

                UnityEngine.UI.Slider slider = bar as UnityEngine.UI.Slider;
                if (slider != null)
                {
                    slider.value = ratio;
                }
                else
                {
                    UnityEngine.UI.Image img = bar as UnityEngine.UI.Image;
                    if (img != null)
                    {
                        img.fillAmount = ratio;
                    }
                }
            }
        }
        catch
        {
            // Swallow reflection issues so the original UI logic continues.
        }
    }
}
