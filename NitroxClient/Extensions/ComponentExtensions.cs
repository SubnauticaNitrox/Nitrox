using UnityEngine;

namespace NitroxClient.Extensions;

public static class ComponentExtensions
{
    extension(Component self)
    {
        /// <summary>
        ///     Returns true if the Unity object is, or part of, a remote player object.
        /// </summary>
        public bool IsRemotePlayer => self.gameObject.IsRemotePlayer;
    }
}
