using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.MonoBehaviours.Debugging;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxBootstrapper : MonoBehaviour
    {
        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<SceneCleanerPreserve>();

            CreateDebugger();
        }

        private void CreateDebugger()
        {
            GameObject debugger = new GameObject();
            debugger.name = "DebuggerManager";
            debugger.AddComponent<NitroxDebug>();
            debugger.transform.SetParent(transform);
        }
    }
}
