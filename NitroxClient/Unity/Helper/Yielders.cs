using UnityEngine;

namespace NitroxClient.Unity.Helper;

/// <summary>
///     Cache for yields in IEnumeration to reduce GC pressure.
/// </summary>
public static class Yielders
{
    public static readonly WaitForFixedUpdate WaitForFixedUpdate = new();
    public static readonly WaitForEndOfFrame WaitForEndOfFrame = new();
    public static readonly WaitForSeconds WaitFor100Milliseconds = new(.1f);
    public static readonly WaitForSeconds WaitForHalfSecond = new(.5f);
    public static readonly WaitForSeconds WaitFor1Second = new(1);
    public static readonly WaitForSeconds WaitFor2Seconds = new(2);
    public static readonly WaitForSeconds WaitFor3Seconds = new(3);
}

