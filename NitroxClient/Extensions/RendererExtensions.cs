using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.Extensions;

public static class RendererExtensions
{
    public static SkinnedMeshRenderer GetRenderer(this GameObject playerModel, string equipmentGameObjectName)
    {
        return playerModel
               .transform
               .Find(equipmentGameObjectName)
               .gameObject
               .GetComponent<SkinnedMeshRenderer>();
    }

    /// Copied from MainMenuLoadButton.ShiftAlpha()
    public static IEnumerator ShiftAlpha(
        this CanvasGroup cg,
        float targetAlpha,
        float animTime,
        float power,
        bool toActive,
        Selectable buttonToSelect = null)
    {
        float start = Time.time;
        while (Time.time - start < animTime)
        {
            cg.alpha = Mathf.Lerp(cg.alpha, targetAlpha, Mathf.Pow(Mathf.Clamp01((Time.time - start) / animTime), power));
            yield return null;
        }
        cg.alpha = targetAlpha;
        if (toActive)
        {
            cg.interactable = true;
            cg.blocksRaycasts = true;
        }
        else
        {
            cg.interactable = false;
            cg.blocksRaycasts = false;
        }
    }
}
