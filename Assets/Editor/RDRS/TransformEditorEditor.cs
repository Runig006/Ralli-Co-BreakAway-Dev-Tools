using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TransformEditor))]
public class TransformEditorEditor : RDRSEditorBase
{
    public override void OnInspectorGUI()
    {
        
        serializedObject.Update();
        this.DrawTagHeader();
        this.PrintInsideAccordion();

        SerializedProperty propertyProp = serializedObject.FindProperty("property");
        SerializedProperty additiveProp = serializedObject.FindProperty("additive");
        
        if (propertyProp != null && additiveProp != null)
        {
            bool isRotation = (TransformEditor.TargetProperty)propertyProp.enumValueIndex == TransformEditor.TargetProperty.Rotation;
            bool isAdditive = additiveProp.boolValue;

            if (isRotation && !isAdditive)
            {
                EditorGUILayout.HelpBox(
                    "In 'Rotation' mode without additive, the axis mask is applied to the Base Value — not to the runtime value. Quaternions VS Euler Angles...and that things",
                    MessageType.Warning
                );
            }
        }
        serializedObject.ApplyModifiedProperties();
    }
}
