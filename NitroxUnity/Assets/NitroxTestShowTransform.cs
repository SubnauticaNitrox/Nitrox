using UnityEditor;
using UnityEngine;

[ExecuteAlways]
public class NitroxTestShowTransform : MonoBehaviour
{
    [Header("GlobalPosition")]
    [SerializeField] private float positionX;
    [SerializeField] private float positionY;
    [SerializeField] private float positionZ;

    [Header("GlobalRotation")]
    [SerializeField] private float rotationX;
    [SerializeField] private float rotationY;
    [SerializeField] private float rotationZ;
    [SerializeField] private float rotationW;

    [Header("GlobalRotationEuler")]
    [SerializeField] private float rotationEulerX;
    [SerializeField] private float rotationEulerY;
    [SerializeField] private float rotationEulerZ;
    
    [Header("Matrices")]
    [SerializeField] private Matrix4x4 localToWorldMatrix;
    [SerializeField] private Matrix4x4 translationMatrix;
    [SerializeField] private Matrix4x4 rotationMatrix;
    [SerializeField] private Matrix4x4 scaleMatrix;

    public void Update()
    {
        Vector3 position = transform.position;
        positionX = position.x;
        positionY = position.y;
        positionZ = position.z;

        Quaternion rotation = transform.rotation;
        rotationX = rotation.x;
        rotationY = rotation.y;
        rotationZ = rotation.z;
        rotationW = rotation.w;

        Vector3 eulerAngles = transform.eulerAngles;
        rotationEulerX = eulerAngles.x;
        rotationEulerY = eulerAngles.y;
        rotationEulerZ = eulerAngles.z;

        localToWorldMatrix = transform.localToWorldMatrix;
        translationMatrix = Matrix4x4.Translate(transform.localPosition);
        rotationMatrix = Matrix4x4.Rotate(transform.localRotation);
        scaleMatrix = Matrix4x4.Scale(transform.localScale);
    }
}

[CustomPropertyDrawer(typeof(Matrix4x4))]
public class MatrixDrawer : PropertyDrawer
{
    private  const float CELL_WIDTH = 95f;
    private  const float CELL_HEIGHT = 20f;
    
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
        
        position.x += 10;
        position.y += 20;

        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 4; j++)
            {
                Rect rect = new Rect(position.x, position.y, CELL_WIDTH, CELL_HEIGHT);
                EditorGUI.PropertyField(rect, property.FindPropertyRelative("e" + i+j), GUIContent.none);
                
                position.x += CELL_WIDTH;
            }

            position.x -= CELL_WIDTH*4;
            position.y += CELL_HEIGHT;
        }
        
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) * 5 + 20;
    }
}
