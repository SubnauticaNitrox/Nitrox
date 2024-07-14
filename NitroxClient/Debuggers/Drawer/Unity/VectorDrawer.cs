using NitroxModel_Subnautica.DataStructures;
using NitroxModel.DataStructures.Unity;
using UnityEngine;

namespace NitroxClient.Debuggers.Drawer.Unity;

public class VectorDrawer : IEditorDrawer<Vector2, VectorDrawer.DrawOptions>, IEditorDrawer<Vector3, VectorDrawer.DrawOptions>, IEditorDrawer<NitroxVector3>, IEditorDrawer<Vector4>, IEditorDrawer<NitroxVector4>, IEditorDrawer<Quaternion>,
                            IEditorDrawer<Int3>
{
    private const float MAX_WIDTH = 400;

    public Vector2 Draw(Vector2 vector2, DrawOptions options)
    {
        options ??= new DrawOptions();

        float valueWidth = options.MaxWidth / 2 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(options.MaxWidth)))
        {
            vector2.x = NitroxGUILayout.FloatField(vector2.x, valueWidth);
            NitroxGUILayout.Separator();
            vector2.y = NitroxGUILayout.FloatField(vector2.y, valueWidth);
            return vector2;
        }
    }

    public Vector3 Draw(Vector3 vector3, DrawOptions options)
    {
        options ??= new DrawOptions();

        float valueWidth = options.MaxWidth / 3 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(options.MaxWidth)))
        {
            vector3.x = NitroxGUILayout.FloatField(vector3.x, valueWidth);
            NitroxGUILayout.Separator();
            vector3.y = NitroxGUILayout.FloatField(vector3.y, valueWidth);
            NitroxGUILayout.Separator();
            vector3.z = NitroxGUILayout.FloatField(vector3.z, valueWidth);
            return vector3;
        }
    }

    public Vector4 Draw(Vector4 vector)
    {
        float valueWidth = MAX_WIDTH / 4 - 6;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(MAX_WIDTH)))
        {
            vector.x = NitroxGUILayout.FloatField(vector.x, valueWidth);
            NitroxGUILayout.Separator();
            vector.y = NitroxGUILayout.FloatField(vector.y, valueWidth);
            NitroxGUILayout.Separator();
            vector.z = NitroxGUILayout.FloatField(vector.z, valueWidth);
            NitroxGUILayout.Separator();
            vector.w = NitroxGUILayout.FloatField(vector.w, valueWidth);
            return vector;
        }
    }

    public Quaternion Draw(Quaternion vector)
    {
        Vector4 vector4 = new(vector.x, vector.y, vector.z, vector.w);
        vector4 = Draw(vector4);
        return new Quaternion(vector4.x, vector4.y, vector4.z, vector4.w);
    }

    public Int3 Draw(Int3 vector)
    {
        float valueWidth = MAX_WIDTH / 3 - 5;
        using (new GUILayout.HorizontalScope(GUILayout.MaxWidth(MAX_WIDTH)))
        {
            vector.x = NitroxGUILayout.IntField(vector.x, valueWidth);
            NitroxGUILayout.Separator();
            vector.y = NitroxGUILayout.IntField(vector.y, valueWidth);
            NitroxGUILayout.Separator();
            vector.z = NitroxGUILayout.IntField(vector.z, valueWidth);
            return vector;
        }
    }

    public NitroxVector3 Draw(NitroxVector3 vector)
    {
        return Draw(vector.ToUnity()).ToDto();
    }

    public NitroxVector4 Draw(NitroxVector4 vector)
    {
        return Draw(vector.ToUnity()).ToDto();
    }

    public record DrawOptions(float MaxWidth = MAX_WIDTH);

    public Vector2 Draw(Vector2 vector)
    {
        return Draw(vector, null);
    }

    public Vector3 Draw(Vector3 vector)
    {
        return Draw(vector, null);
    }
}
