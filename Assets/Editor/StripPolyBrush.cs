// Editor/StripPolybrush.cs
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class StripPolybrush
{
    static readonly string[] TargetTypeNames = { "PolybrushMesh", "z_AdditionalVertexStreams" };

    [MenuItem("Tools/Polybrush/Remove from the actual scene")]
    public static void StripFromCurrentScene()
    {
        int removed = 0;
        foreach (GameObject go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (!go.scene.IsValid() || EditorUtility.IsPersistent(go))
            {
                continue;
            }

            removed += StripOnGameObject(go);
        }

        EditorSceneManager.MarkAllScenesDirty();
        EditorUtility.DisplayDialog("Polybrush", $"Components deleted: {removed}", "OK");
    }

    [MenuItem("Tools/Polybrush/Remove from all the prefabs")]
    public static void StripFromAllPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        int totalRemoved = 0, touchedPrefabs = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject root = PrefabUtility.LoadPrefabContents(path);
            int removedHere = 0;

            foreach (Component c in root.GetComponentsInChildren<Component>(true))
            {
                removedHere += TryRemoveIfPolybrush(c);
            }

            if (removedHere > 0)
            {
                foreach (MeshRenderer mr in root.GetComponentsInChildren<MeshRenderer>(true))
                {
                    if (mr.additionalVertexStreams != null)
                    {
                        mr.additionalVertexStreams = null;
                    }
                }

                PrefabUtility.SaveAsPrefabAsset(root, path);
                touchedPrefabs++;
                totalRemoved += removedHere;
            }
            PrefabUtility.UnloadPrefabContents(root);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Polybrush",  $"Prefabs edited: {touchedPrefabs}\nComponents deleted: {totalRemoved}", "OK");
    }

    static int StripOnGameObject(GameObject go)
    {
        int removed = 0;

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        if (mr && mr.additionalVertexStreams != null)
        {
            mr.additionalVertexStreams = null;
        }

        foreach (Component c in go.GetComponents<Component>())
        {
            removed += TryRemoveIfPolybrush(c);
        }

        return removed;
    }

    static int TryRemoveIfPolybrush(Component c)
    {
        if (c == null)
        {
            return 0;
        }
        Type t = c.GetType();
        String name = t.Name;
        String full = t.FullName ?? "";

        bool isTarget = TargetTypeNames.Any(n => name == n || full.EndsWith("." + n, StringComparison.Ordinal));
        if (!isTarget)
        {
            return 0;
        }

        Undo.RegisterCompleteObjectUndo(c.gameObject, "Remove Polybrush component");
        UnityEngine.Object.DestroyImmediate(c, true);
        return 1;
    }
}
