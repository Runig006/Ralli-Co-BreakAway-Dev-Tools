using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomEditor(typeof(RDRSNode), true)]
public class RDRSEditorBase : Editor
{
    protected bool showProperties = false;

    private string copyMessage = null;
    private float copyMessageTime;
    private const float copyMessageDuration = 2f;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        this.DrawTagHeader();
        this.PrintInsideAccordion();

        serializedObject.ApplyModifiedProperties();
    }

    //Tag Function
    protected virtual void DrawTagHeader()
    {
        SerializedProperty tagProp = serializedObject.FindProperty("Tag");

        if (tagProp != null)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(tagProp);
            if (GUILayout.Button("Copy", GUILayout.Width(40)))
            {
                if (target is RDRSNode rdrs)
                {
                    RDRSCopyPasteBuffer.CopiedReference = rdrs;
                    string messageName = !string.IsNullOrEmpty(rdrs.Tag) ? rdrs.Tag : $"({rdrs.GetType().Name})";
                    this.copyMessage = $"References to a \"{messageName}\" copy";
                    this.copyMessageTime = Time.realtimeSinceStartup;
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        if (!string.IsNullOrEmpty(copyMessage) && Time.realtimeSinceStartup - copyMessageTime < copyMessageDuration)
        {
            EditorGUILayout.HelpBox(copyMessage, MessageType.Info);
        }
        
        SerializedProperty frequencyProp = serializedObject.FindProperty("frequency");
        if(frequencyProp != null)
        {
            EditorGUILayout.PropertyField(frequencyProp);
        }

        if (tagProp != null || frequencyProp != null)
        {
            GUILayout.Space(2f);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Space(2f);
        }
        
    }

    //Print function inside a tab
    protected virtual void PrintInsideAccordion()
    {
        string foldoutKey = $"RDRS_Foldout_{target.GetInstanceID()}";
        bool stored = SessionState.GetBool(foldoutKey, this.showProperties);
    
        this.showProperties = EditorGUILayout.Foldout(stored, "Properties", true);
        SessionState.SetBool(foldoutKey, this.showProperties);
        if (this.showProperties)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Space(12);
            GUILayout.BeginVertical();
        
            Type targetType = target.GetType();
            FieldInfo[] fields = targetType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            foreach (FieldInfo field in fields)
            {
                if (field.Name == "Tag" || field.Name == "frequency" || field.IsNotSerialized || field.IsDefined(typeof(HideInInspector)))
                {
                    continue;
                }

                this.DrawCustomField(field);
            }
            
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

    }

    protected void DrawCustomField(FieldInfo field)
    {
        SerializedProperty prop = serializedObject.FindProperty(field.Name);
        if (prop == null)
        {
            return;
        }

        if (typeof(RDRSNode).IsAssignableFrom(field.FieldType))
        {
            this.DrawFieldWithOptionalTag(prop, field);
        }
        else if (field.FieldType.IsArray && typeof(RDRSNode).IsAssignableFrom(field.FieldType.GetElementType()))
        {
            this.DrawArrayWithPaste(prop, field);
        }
        else
        {
            EditorGUILayout.PropertyField(prop, true);
        }
    }

    //Print any "normal" field
    protected virtual void DrawFieldWithOptionalTag(SerializedProperty prop, FieldInfo field)
    {
        Type fieldType = field.FieldType;
        UnityEngine.Object obj = prop.objectReferenceValue;

        string label = ObjectNames.NicifyVariableName(field.Name);

        if (obj is RDRSNode rdrs && !string.IsNullOrEmpty(rdrs.Tag))
        {
            label += $" → {rdrs.Tag}";
        }

        EditorGUILayout.BeginHorizontal();

        prop.objectReferenceValue = EditorGUILayout.ObjectField(label, obj, fieldType, true);

        /** PASTE BUTTON **/
        if (GUILayout.Button("Paste", GUILayout.Width(50)))
        {
            RDRSNode pasted = RDRSCopyPasteBuffer.CopiedReference;
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
            RDRSNode pasted = RDRSCopyPasteBuffer.CopiedReference;
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
}


[CustomPropertyDrawer(typeof(RDRSNode), true)]
public class RDRSReaderBaseDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty objRef = property.FindPropertyRelative("m_Script") != null ? property.FindPropertyRelative("m_Script") : property.Copy();
        UnityEngine.Object obj = property.objectReferenceValue;
        if (obj is RDRSNode rdrs && !string.IsNullOrEmpty(rdrs.Tag))
        {
            label.text = rdrs.Tag;
        }
        EditorGUI.PropertyField(position, property, label, true);
    }
}