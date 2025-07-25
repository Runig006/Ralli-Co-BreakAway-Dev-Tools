using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomEditor(typeof(FixedValueReader))]
public class FixedValueReaderEditor : RDRSEditorBase
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        this.DrawTagHeader();
        SerializedProperty valueTypeProp = serializedObject.FindProperty("valueType");
        EditorGUILayout.PropertyField(valueTypeProp);

        FixedValueReader.FixedReaderValueType valueType = (FixedValueReader.FixedReaderValueType)valueTypeProp.enumValueIndex;

        switch (valueType)
        {
            case FixedValueReader.FixedReaderValueType.Float:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("floatValue"));
                break;
            case FixedValueReader.FixedReaderValueType.String:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("stringValue"));
                break;
            case FixedValueReader.FixedReaderValueType.Vector2:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vector2Value"));
                break;
            case FixedValueReader.FixedReaderValueType.Vector3:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("vector3Value"));
                break;
            case FixedValueReader.FixedReaderValueType.Object:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectValue"));
                break;
            case FixedValueReader.FixedReaderValueType.ObjectArray:
                EditorGUILayout.PropertyField(serializedObject.FindProperty("objectsValue"), true);
                break;
            default:
                EditorGUILayout.HelpBox("Unsupported value type.", MessageType.Warning);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}