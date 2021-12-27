using System.Linq;
using System.Reflection;
using LitJson;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using UnityEngine;

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
            object value = JsonMapper.ToObject<object>(sceneDebuggerChange.Value);

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
                    field.SetValue(component, value);
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
                        gameObject.position = (Vector3)value;
                        break;

                    case "rotation":
                        gameObject.rotation = (Quaternion)value;
                        break;

                    case "scale":
                        gameObject.localScale = (Vector3)value;
                        break;
                    case "enabled":
                        gameObject.gameObject.SetActive((bool)value);
                        break;
                }
            }

        }
    }
}
