using UnityEngine;
using UnityEditor;
using System.IO;

public class MeshCombinerEditor : MonoBehaviour
{
    [MenuItem("Tools/Combine Meshes From Selection")]
    private static void CombineSelectedMeshes()
    {
        GameObject selected = Selection.activeGameObject;

        if (selected == null)
        {
            Debug.LogWarning("Please select a GameObject with children that contain MeshFilters.");
            return;
        }

        MeshFilter[] meshFilters = selected.GetComponentsInChildren<MeshFilter>();
        if (meshFilters.Length == 0)
        {
            Debug.LogWarning("No MeshFilters found in selected object's children.");
            return;
        }

        CombineInstance[] combineInstances = new CombineInstance[meshFilters.Length];
        Material sharedMaterial = null;

        for (int i = 0; i < meshFilters.Length; i++)
        {
            MeshFilter meshFilter = meshFilters[i];
            if (meshFilter.sharedMesh == null)
            {
                continue;
            }

            combineInstances[i].mesh = meshFilter.sharedMesh;
            combineInstances[i].transform = selected.transform.worldToLocalMatrix * meshFilter.transform.localToWorldMatrix;

            if (sharedMaterial == null)
            {
                MeshRenderer meshRenderer = meshFilter.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    sharedMaterial = meshRenderer.sharedMaterial;
                }
            }
        }

        GameObject combinedObject = new GameObject(selected.name + "_Combined");
        combinedObject.transform.position = selected.transform.position;
        combinedObject.transform.rotation = selected.transform.rotation;
        combinedObject.transform.localScale = selected.transform.localScale;

        Mesh combinedMesh = new Mesh();
        combinedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        combinedMesh.CombineMeshes(combineInstances);

        // Ensure target folder exists
        string folderPath = "Assets/GeneratedMeshes";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", "GeneratedMeshes");
        }

        string meshAssetPath = folderPath + "/" + selected.name + "_CombinedMesh.asset";
        AssetDatabase.CreateAsset(combinedMesh, meshAssetPath);
        AssetDatabase.SaveAssets();

        Mesh savedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshAssetPath);

        MeshFilter combinedMeshFilter = combinedObject.AddComponent<MeshFilter>();
        combinedMeshFilter.sharedMesh = savedMesh;

        MeshRenderer combinedMeshRenderer = combinedObject.AddComponent<MeshRenderer>();
        combinedMeshRenderer.sharedMaterial = sharedMaterial;

        Selection.activeGameObject = combinedObject;
        SceneView.lastActiveSceneView.FrameSelected();

        Debug.Log("Meshes combined and saved as asset: " + meshAssetPath);
    }
}
