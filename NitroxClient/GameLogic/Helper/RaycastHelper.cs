using System;
using System.Buffers;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic.Helper;

internal static class RaycastHelper
{
    private static readonly IComparer<RaycastHit> rayHitDistanceComparer = Comparer<RaycastHit>.Create((a, b) => a.distance.CompareTo(b.distance));
    private static Camera? MainCamera => field = field.AliveOrNull() ? field : Camera.main;

    public static RaycastHit? GetClosestHitFromAim(float maxDistance, float minDistance = 3, int hitBufferSize = 64)
    {
        if (!MainCamera)
        {
            return null;
        }

        Ray ray = MainCamera.ScreenPointToRay(Input.mousePosition);
        int layerMask = ~LayerMask.GetMask("UI", "Ignore Raycast");
        RaycastHit[] hits = ArrayPool<RaycastHit>.Shared.Rent(hitBufferSize);
        try
        {
            int hitAmount = Physics.RaycastNonAlloc(ray, hits, maxDistance, layerMask);
            if (hitAmount > 0)
            {
                Array.Sort(hits, 0, hitAmount, rayHitDistanceComparer);
                for (int i = 0; i < hitAmount; i++)
                {
                    RaycastHit hit = hits[i];
                    if (hit.collider.isTrigger || hit.distance < minDistance)
                    {
                        continue;
                    }
                    return hit;
                }
            }
        }
        finally
        {
            ArrayPool<RaycastHit>.Shared.Return(hits);
        }
        return null;
    }
}
