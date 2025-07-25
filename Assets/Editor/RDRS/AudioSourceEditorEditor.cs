using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomEditor(typeof(AudioSourceEditor))]
public class AudioSourceEditorEditor : RDRSEditorBase
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        this.DrawTagHeader();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("audioReaders"), true);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("valueReader"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("propertyToEdit"));

        SerializedProperty propertyToEditProp = serializedObject.FindProperty("propertyToEdit");
        AudioSourceEditor.AudioProperty selectedProperty = (AudioSourceEditor.AudioProperty)propertyToEditProp.enumValueIndex;

        if (selectedProperty == AudioSourceEditor.AudioProperty.PlayState)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Play Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("playStrategy"));

            
            this.DrawFadeEnumWithFloat(
                serializedObject.FindProperty("playVolumeStrategy"),
                AudioSourceEditor.PlayVolumeStrategy.FadeIn,
                serializedObject.FindProperty("fadeInDuration")
            );

            this.DrawFadeEnumWithFloat(
                serializedObject.FindProperty("stopStrategy"),
                AudioSourceEditor.StopStrategy.FadeOut,
                serializedObject.FindProperty("fadeOutDuration")
            );
        }

        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawFadeEnumWithFloat(SerializedProperty enumProp, Enum targetValue, SerializedProperty floatProp)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PropertyField(enumProp);

        if (enumProp.enumValueIndex == Convert.ToInt32(targetValue))
        {
            floatProp.floatValue = EditorGUILayout.FloatField(floatProp.floatValue, GUILayout.Width(50));
        }

        EditorGUILayout.EndHorizontal();
    }
}