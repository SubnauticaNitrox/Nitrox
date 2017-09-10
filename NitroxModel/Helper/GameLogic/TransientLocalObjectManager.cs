using NitroxModel.DataStructures.Util;
using System.Collections.Generic;

namespace NitroxModel.Helper.GameLogic
{
    /**
     * Class used for temporarily storing variables local to patched methods.  Certain circumstances require that these
     * be referenced at a later point and most of the time it is too prohibitive to expose global statics. 
     * 
     * An example use-case is the created gameobject from the vehicle constructor class.  This gameobject is only accessible
     * locally when crafted.  We need to access it at future times to retrieve and set its GUID.  
     */
    public static class TransientLocalObjectManager
    {
        public enum TransientObjectType
        {
            CONSTRUCTOR_INPUT_CRAFTED_GAMEOBJECT,
            BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT
        }

        public static Dictionary<TransientObjectType, object> localObjectsById = new Dictionary<TransientObjectType, object>();

        public static void Add(TransientObjectType key, object o)
        {
            localObjectsById[key] = o;
        }

        public static Optional<object> Get(TransientObjectType key)
        {
            if(localObjectsById.ContainsKey(key))
            {
                return Optional<object>.OfNullable(localObjectsById[key]);
            }

            return Optional<object>.Empty();
        }
    }
}
