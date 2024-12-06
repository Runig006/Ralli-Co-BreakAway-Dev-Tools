using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TrackExporter : MonoBehaviour
{
	private const string LastFolderKey = "TrackExporter_LastFolderPath";

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
		
		// Obtener la última carpeta utilizada
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

		HashSet<string> dependencies = new HashSet<string>(FilterAssets(prefabPath));
		dependencies.Add(prefabPath); 
		
		List<string> uniqueAssets = new List<string>(dependencies);
		
		string bundleName = selectedObject.name.ToLower() + ".track";
		BuildCircuitAssetBundle(uniqueAssets, bundleName, folderPath);
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