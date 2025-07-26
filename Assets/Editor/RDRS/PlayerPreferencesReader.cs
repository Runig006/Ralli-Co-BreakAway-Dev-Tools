using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;


[CustomEditor(typeof(PlayerPreferencesReader))]
public class PlayerPreferencesReaderEditor : RDRSEditorBase
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); 
        if (this.showProperties)
        {
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Debug preferences", EditorStyles.boldLabel);
            PlayerPreferencesReader.preferredSpeedUnit = (PlayerPreferencesReader.SpeedUnit)EditorGUILayout.EnumPopup("Speed Unit", PlayerPreferencesReader.preferredSpeedUnit);
            PlayerPreferencesReader.preferredTemperatureUnit = (PlayerPreferencesReader.TemperatureUnit)EditorGUILayout.EnumPopup("Temperature Unit", PlayerPreferencesReader.preferredTemperatureUnit);
            PlayerPreferencesReader.preferredCarColor = EditorGUILayout.ColorField("Car Color", PlayerPreferencesReader.preferredCarColor);
        }
    }
}