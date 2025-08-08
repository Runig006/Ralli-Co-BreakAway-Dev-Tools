using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class ShaderMaterialFinder : EditorWindow
{
    Shader targetShader;
    List<Material> foundMaterials = new List<Material>();

    [MenuItem("Tools/Find Materials By Shader")]
    public static void ShowWindow()
    {
        GetWindow<ShaderMaterialFinder>("Find Materials By Shader");
    }

    void OnGUI()
    {
        targetShader = (Shader)EditorGUILayout.ObjectField("Shader", targetShader, typeof(Shader), false);

        if (GUILayout.Button("Find Materials"))
        {
            FindMaterials();
        }

        if (foundMaterials.Count > 0)
        {
            EditorGUILayout.LabelField("Found Materials:");
            foreach (var mat in foundMaterials)
            {
                EditorGUILayout.ObjectField(mat, typeof(Material), false);
            }
        }
    }

    void FindMaterials()
    {
        foundMaterials.Clear();
        string[] materialGUIDs = AssetDatabase.FindAssets("t:Material");
        foreach (string guid in materialGUIDs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat != null && mat.shader == targetShader)
            {
                foundMaterials.Add(mat);
            }
        }

        Debug.Log($"Found {foundMaterials.Count} materials using shader: {targetShader.name}");
    }
}
