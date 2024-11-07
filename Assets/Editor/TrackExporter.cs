using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class TrackExporter : MonoBehaviour
{
	[MenuItem("Assets/Export Track")]
	static void ExportSelectedPrefab()
	{
		Object selectedObject = Selection.activeObject;
		if (selectedObject == null || !(selectedObject is GameObject))
		{
			Debug.LogError("Please select a track first");
			return;
		}
		
		string folderPath = EditorUtility.SaveFolderPanel("Select a folder to save the track (Remember that it has to be GameLocation/Tracks/CollectionFolder, so it can be read for the game)", "", "");
		
		
		if (string.IsNullOrEmpty(folderPath))
		{
		   return;
		}

		// Get the prefab path and filter dependencies
		string prefabPath = AssetDatabase.GetAssetPath(selectedObject);
		HashSet<string> dependencies = new HashSet<string>(FilterAssets(prefabPath));  // Use HashSet to ensure unique assets
		dependencies.Add(prefabPath); 
		
		List<string> uniqueAssets = new List<string>(dependencies);
		
		// Create the asset bundle
		string bundleName = selectedObject.name.ToLower() + ".track";
		BuildCircuitAssetBundle(uniqueAssets, bundleName, folderPath);
	}
	
	static HashSet<string> FilterAssets(string prefabPath)
	{
		HashSet<string> filteredDependencies = new HashSet<string>();  // HashSet to prevent duplicate entries
		foreach (var dependency in AssetDatabase.GetDependencies(prefabPath, true))
		{
			// Exclude scripts and editor/package files
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
