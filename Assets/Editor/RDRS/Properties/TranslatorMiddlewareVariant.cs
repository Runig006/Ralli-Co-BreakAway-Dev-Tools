using UnityEditor;
using UnityEngine;
using System;
using System.Reflection;

[CustomPropertyDrawer(typeof(TranslatorMiddleware.TranslatorMiddlewareVariant))]
public class TranslatorMiddlewareVariantDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);
        SerializedProperty typeProperty = this.GetFieldType(property);

        if (typeProperty == null)
        {
            EditorGUI.LabelField(position, "Unable to resolve SupportedType");
            EditorGUI.EndProperty();
            return;
        }

        TranslatorMiddleware.SupportedType type = (TranslatorMiddleware.SupportedType)typeProperty.enumValueIndex;
        string fieldName = this.GetFieldNameForType(type);
        SerializedProperty targetProp = property.FindPropertyRelative(fieldName);
        EditorGUI.PropertyField(position, targetProp, label, true);

        EditorGUI.EndProperty();
    }

    private string GetFieldNameForType(TranslatorMiddleware.SupportedType type)
    {
        return type switch
        {
            TranslatorMiddleware.SupportedType.Float => "floatValue",
            TranslatorMiddleware.SupportedType.Color => "colorValue",
            TranslatorMiddleware.SupportedType.String => "stringValue",
            TranslatorMiddleware.SupportedType.GameObject => "gameObjectValue",
            TranslatorMiddleware.SupportedType.AudioClip => "audioClipValue",
            _ => ""
        };
    }

    // We are in the property, not the editor
    private SerializedProperty GetFieldType(SerializedProperty variantProperty)
    {
        SerializedProperty parent = GetParentProperty(variantProperty);
        SerializedProperty typeProp = variantProperty.name.Contains("input") ? parent.serializedObject.FindProperty("inputType") : parent.serializedObject.FindProperty("outputType");
        return typeProp;
    }

    private SerializedProperty GetParentProperty(SerializedProperty prop)
    {
        string path = prop.propertyPath;
        int lastDot = path.LastIndexOf('.');
        if (lastDot < 0)
        {
            return null;
        }

        string parentPath = path.Substring(0, lastDot);
        return prop.serializedObject.FindProperty(parentPath);
    }
}