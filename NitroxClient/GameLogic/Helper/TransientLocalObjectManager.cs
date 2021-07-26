using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Util;

namespace NitroxClient.GameLogic.Helper
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
            BASE_GHOST_NEWLY_CONSTRUCTED_BASE_GAMEOBJECT,
            
            LATEST_DECONSTRUCTED_BASE_PIECE,
            LATEST_DECONSTRUCTED_BASE_PIECE_GUID
        }

        public static Dictionary<TransientObjectType, object> localObjectsById = new Dictionary<TransientObjectType, object>();

        public static void Add(TransientObjectType key, object o)
        {
            localObjectsById[key] = o;
        }

        public static void Remove(TransientObjectType key)
        {
            localObjectsById.Remove(key);
        }

        public static Optional<object> Get(TransientObjectType key)
        {
            object obj;
            localObjectsById.TryGetValue(key, out obj);
            return Optional.OfNullable(obj);
        }

        public static T Require<T>(TransientObjectType key)
        {
            object obj;
            if (!localObjectsById.TryGetValue(key, out obj))
            {
                throw new Exception("Did not have an entry for key: " + key);
            }
            
            return (T)obj;
        }
    }
}
