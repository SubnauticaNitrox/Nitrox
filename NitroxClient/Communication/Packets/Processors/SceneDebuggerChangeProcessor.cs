using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
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
            GameObject gameObject = GameObject.Find(sceneDebuggerChange.Path);
            if (sceneDebuggerChange.GameObjectID != -1)
            {
                gameObject = gameObject.transform.parent.GetChild(sceneDebuggerChange.GameObjectID).gameObject;
            }
            Component component = gameObject.GetComponents<Component>()[sceneDebuggerChange.ComponentID];
            Log.Debug(component.GetType());
            if (component.GetType() != typeof(Transform))
            {
                FieldInfo field = component.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic).Where(fi => fi.Name == sceneDebuggerChange.FieldName).ToArray()[0];
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
                if (sceneDebuggerChange.FieldName == "position")
                {
                    gameObject.transform.position = (Vector2)sceneDebuggerChange.Value;
                }
                else if (sceneDebuggerChange.FieldName == "rotation")
                {
                    gameObject.transform.rotation = (Quaternion)sceneDebuggerChange.Value;
                }
                else if (sceneDebuggerChange.FieldName == "scale")
                {
                    gameObject.transform.localScale = (Vector2)sceneDebuggerChange.Value;
                }
            }

        }
    }
}
