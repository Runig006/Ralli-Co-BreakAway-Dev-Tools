using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TrackExporter : MonoBehaviour
{
	private const string LastFolderKey = "TrackExporter_LastFolderPath";
	private const string TempPrefab = "Assets/__TempExportedTrack.prefab";

	[MenuItem("Assets/Export Track")]
	static void ExportSelectedPrefab()
	{
		Object selectedObject = Selection.activeObject;
		string prefabPath = AssetDatabase.GetAssetPath(selectedObject);
		
		if (selectedObject == null || !(selectedObject is GameObject))
		{
			Debug.LogError("Please select a track first");
			return;
		}
		
		if (PrefabUtility.GetPrefabAssetType(selectedObject) == PrefabAssetType.NotAPrefab)
		{
			Debug.LogError("The selected object is not a valid prefab.");
			return;
		}
		
		if (string.IsNullOrEmpty(prefabPath))
		{
			Debug.LogError("Could not determine the path of the selected asset.");
			return;
		}	
		
		// Get the last folder used
		string lastUsedFolder = EditorPrefs.GetString(LastFolderKey, "");
		string folderPath = EditorUtility.SaveFolderPanel(
			"Select a folder to save the track (Remember that it has to be GameLocation/Tracks/CollectionFolder, so it can be read for the game)", 
			lastUsedFolder, 
			""
		);
		
		if (string.IsNullOrEmpty(folderPath))
		{
		   return;
		}

		EditorPrefs.SetString(LastFolderKey, folderPath);
		
		//Create the temp prefab, and add the snapshotHolder
		GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject tempInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        
        if (tempInstance.GetComponent<Level>() == null)
		{
			Debug.LogError("The prefab must contain a Level component.");
			GameObject.DestroyImmediate(tempInstance);
			return;
		}

        SnapshotHolder snapshot = tempInstance.GetComponent<SnapshotHolder>();
		if (snapshot == null)
		{
			snapshot = tempInstance.AddComponent<SnapshotHolder>();
		}
        snapshot.CaptureSnapshot();
		PrefabUtility.SaveAsPrefabAsset(tempInstance, TempPrefab);
		
		//Use the tempPrefab to save
		HashSet<string> dependencies = new HashSet<string>(FilterAssets(TempPrefab));
		dependencies.Add(TempPrefab); 
		
		List<string> uniqueAssets = new List<string>(dependencies);
		
		string bundleName = selectedObject.name.ToLower() + ".track";
		BuildCircuitAssetBundle(uniqueAssets, bundleName, folderPath);
		
		//Clean
		AssetDatabase.DeleteAsset(TempPrefab);
        GameObject.DestroyImmediate(tempInstance);
	}
	
	static HashSet<string> FilterAssets(string prefabPath)
	{
		HashSet<string> filteredDependencies = new HashSet<string>();
		foreach (var dependency in AssetDatabase.GetDependencies(prefabPath, true))
		{
			if (dependency.EndsWith(".cs") || dependency.Contains("/Editor/") || dependency.Contains("/Packages/"))
			{
				continue;
			}
			filteredDependencies.Add(dependency);
		}
		return filteredDependencies;
	}

	static void BuildCircuitAssetBundle(List<string> assetPaths, string bundleName, string outputPath)
	{
		string tempOutputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		string sourceBundlePath = Path.Combine(tempOutputPath, bundleName);
		string finalBundlePath = Path.Combine(outputPath, bundleName);
		
		Directory.CreateDirectory(tempOutputPath);

		AssetBundleBuild bundleBuild = new AssetBundleBuild
		{
			assetBundleName = bundleName,
			assetNames = assetPaths.ToArray(),
		};

		BuildPipeline.BuildAssetBundles(tempOutputPath, new AssetBundleBuild[] { bundleBuild }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
		
		if (File.Exists(finalBundlePath))
		{
			File.Delete(finalBundlePath);
		}
		
		if (File.Exists(sourceBundlePath))
		{
			File.Move(sourceBundlePath, finalBundlePath);
			Directory.Delete(tempOutputPath, true);
			Debug.Log(bundleName + " Exported in: " + outputPath + ". Have Fun :D");
		}
		else
		{
			Debug.LogError("Failed to export " + bundleName + ".");
		}
	}
}