using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
using NitroxClient.Communication.Abstract;

namespace NitroxClient.Communication.Packets.Processors
{
    class SceneDebuggerChangeProcessor : ClientPacketProcessor<SceneDebuggerChange>
    {
        private readonly IPacketSender packetSender;

        public SceneDebuggerChangeProcessor(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public override void Process(SceneDebuggerChange sceneDebuggerChange)
        {
            Transform gameObject = GameObject.Find(sceneDebuggerChange.Path).transform;
            if (sceneDebuggerChange.GameObjectID != -1)
            {
                gameObject = gameObject.parent.GetChild(sceneDebuggerChange.GameObjectID);
            }
            Component component = gameObject.GetComponents<Component>()[sceneDebuggerChange.ComponentID];

            if (component.GetType() != typeof(Transform))
            {
                FieldInfo field = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).First(fi => fi.Name == sceneDebuggerChange.FieldName);
                try
                {
                    field.SetValue(component, sceneDebuggerChange.Value);
                }
                catch
                {
                    Log.Error("SceneDebuggerChange: SetValue has trown a error");
                }
            }
            else
            {
                switch (sceneDebuggerChange.FieldName)
                {
                    case "position":
                        gameObject.position = (Vector3)sceneDebuggerChange.Value;
                        break;

                    case "rotation":
                        gameObject.rotation = (Quaternion)sceneDebuggerChange.Value;
                        break;

                    case "scale":
                        gameObject.localScale = (Vector3)sceneDebuggerChange.Value;
                        break;
                    case "enabled":
                        gameObject.gameObject.SetActive((bool)sceneDebuggerChange.Value);
                        break;
                }
            }

        }
    }
}
