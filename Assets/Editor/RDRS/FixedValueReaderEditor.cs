using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FixedValueReader))]
public class FixedValueReaderEditor : RDRSEditorBase
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        this.DrawTagHeader();

        SerializedProperty valueTypeProp = serializedObject.FindProperty("valueType");
        FixedValueReader.FixedReaderValueType valueType = (FixedValueReader.FixedReaderValueType)valueTypeProp.enumValueIndex;

        if (valueType == FixedValueReader.FixedReaderValueType.ObjectArray)
        {
            EditorGUILayout.PropertyField(valueTypeProp);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("objectsValue"), GUIContent.none);
        }
        else
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(valueTypeProp, GUIContent.none, GUILayout.MaxWidth(100));
            this.DrawValueFieldInline(valueType);
            EditorGUILayout.EndHorizontal();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawValueFieldInline(FixedValueReader.FixedReaderValueType valueType)
    {
        switch (valueType)
        {
            case FixedValueReader.FixedReaderValueType.Float:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"), GUIContent.none);
                break;
            case FixedValueReader.FixedReaderValueType.String:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"), GUIContent.none);
                break;
            case FixedValueReader.FixedReaderValueType.Vector2:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"), GUIContent.none);
                break;
            case FixedValueReader.FixedReaderValueType.Vector3:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vector3Value"), GUIContent.none);
                break;
            case FixedValueReader.FixedReaderValueType.Object:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectValue"), GUIContent.none);
                break;
            default:
                EditorGUILayout.LabelField("Unsupported Type");
                break;
        }
    }
}
