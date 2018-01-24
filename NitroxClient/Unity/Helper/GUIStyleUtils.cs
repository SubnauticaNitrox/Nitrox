using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Unity.Helper
{
    public static class GUIStyleUtils
    {
        public static GUIStyle CopyModify(this GUIStyle style, Action<GUIStyle> modify)
        {
            GUIStyle copy = new GUIStyle(style);
            modify(copy);
            return copy;
        }
    }
}
