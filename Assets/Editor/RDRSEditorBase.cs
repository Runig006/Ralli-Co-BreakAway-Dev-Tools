using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

public static class RDRSCopyPasteBuffer
{
    public static RDRSReaderBase CopiedReference;
}

[CustomEditor(typeof(RDRSReaderBase), true)]
public class RDRSEditorBase : Editor
{
    private bool showProperties = false;

    private string copyMessage = null;
    private float copyMessageTime;
    private const float copyMessageDuration = 2f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        SerializedProperty tagProp = serializedObject.FindProperty("Tag");

        if (tagProp != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(tagProp);
            if (GUILayout.Button("Copy", GUILayout.Width(40)))
            {
                if (target is RDRSReaderBase rdrs)
                {
                    RDRSCopyPasteBuffer.CopiedReference = rdrs;
                    string messageName = !string.IsNullOrEmpty(rdrs.Tag) ? rdrs.Tag : $"({rdrs.GetType().Name})";
                    this.copyMessage = $"References to a \"{messageName}\" copy";
                    this.copyMessageTime = Time.realtimeSinceStartup;
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(6f);
        }

        if (!string.IsNullOrEmpty(copyMessage) && Time.realtimeSinceStartup - copyMessageTime < copyMessageDuration)
        {
            EditorGUILayout.HelpBox(copyMessage, MessageType.Info);
        }

        this.showProperties = EditorGUILayout.Foldout(this.showProperties, "Properties", true);
        if (this.showProperties)
        {
            Type targetType = target.GetType();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.Name == "Tag" || field.IsNotSerialized || field.IsDefined(typeof(HideInInspector)))
                    continue;

                SerializedProperty prop = serializedObject.FindProperty(field.Name);
                if (prop == null)
                    continue;

                if (typeof(RDRSReaderBase).IsAssignableFrom(field.FieldType))
                {
                    this.DrawFieldWithOptionalTag(prop, field);
                }
                else if (field.FieldType.IsArray && typeof(RDRSReaderBase).IsAssignableFrom(field.FieldType.GetElementType()))
                {
                    DrawArrayWithPaste(prop, field);
                }
                else
                {
                    EditorGUILayout.PropertyField(prop, true);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    //Print any "normal" field
    protected virtual void DrawFieldWithOptionalTag(SerializedProperty prop, FieldInfo field)
    {
        Type fieldType = field.FieldType;
        UnityEngine.Object obj = prop.objectReferenceValue;

        string label = ObjectNames.NicifyVariableName(field.Name);

        if (obj is RDRSReaderBase rdrs && !string.IsNullOrEmpty(rdrs.Tag))
        {
            label += $" → {rdrs.Tag}";
        }

        EditorGUILayout.BeginHorizontal();

        prop.objectReferenceValue = EditorGUILayout.ObjectField(label, obj, fieldType, true);

        /** PASTE BUTTON **/
        if (GUILayout.Button("Paste", GUILayout.Width(50)))
        {
            RDRSReaderBase pasted = RDRSCopyPasteBuffer.CopiedReference;
            if (pasted != null && fieldType.IsAssignableFrom(pasted.GetType()))
            {
                prop.objectReferenceValue = pasted;
            }
            else
            {
                Debug.LogWarning($"Cannot paste: the type {pasted?.GetType().Name ?? "null"} is not compatible with {fieldType.Name}");
                EditorUtility.DisplayDialog("Paste Error", "The copied reference is not compatible with this field.", "OK");
            }
        }
        /****************/

        EditorGUILayout.EndHorizontal();
    }

    //Print a normal array...with a button. Yeah...
    protected void DrawArrayWithPaste(SerializedProperty arrayProp, FieldInfo field)
    {
        Type fieldType = field.FieldType;
        EditorGUILayout.PropertyField(arrayProp, true);

        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Paste", GUILayout.Width(60)))
        {
            RDRSReaderBase pasted = RDRSCopyPasteBuffer.CopiedReference;
            Type elementType = fieldType.GetElementType();
            if (pasted != null && elementType.IsAssignableFrom(pasted.GetType()))
            {
                int index = arrayProp.arraySize;
                arrayProp.InsertArrayElementAtIndex(index);
                arrayProp.GetArrayElementAtIndex(index).objectReferenceValue = pasted;
            }
            else
            {
                Debug.LogWarning($"Cannot paste: the type {pasted?.GetType().Name ?? "null"} is not compatible with {fieldType.Name}");
                EditorUtility.DisplayDialog("Paste Error", "The copied reference is not compatible with this field.", "OK");
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private bool IsRDRSSpecialField(Type type)
    {
        return typeof(RDRSReaderBase).IsAssignableFrom(type) || typeof(IRDRSReader).IsAssignableFrom(type) || typeof(IRDRSExecutor).IsAssignableFrom(type);
    }
}

[CustomEditor(typeof(RDRSExecutorBase), true)]
public class RDRSExecutorBaseEditor : RDRSEditorBase { }

[CustomPropertyDrawer(typeof(RDRSReaderBase), true)]
public class RDRSReaderBaseDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty objRef = property.FindPropertyRelative("m_Script") != null ? property.FindPropertyRelative("m_Script") : property.Copy();
        UnityEngine.Object obj = property.objectReferenceValue;
        if (obj is RDRSReaderBase rdrs && !string.IsNullOrEmpty(rdrs.Tag))
        {
            label.text = rdrs.Tag;
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}

