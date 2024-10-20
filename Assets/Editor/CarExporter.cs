using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CarExporter : MonoBehaviour
{
	[MenuItem("Assets/Export Car")]
	static void ExportSelectedPrefab()
	{
		Object selectedObject = Selection.activeObject;
		if (selectedObject == null || !(selectedObject is GameObject))
		{
			Debug.LogError("Please select a Car first");
			return;
		}
		
		string folderPath = EditorUtility.SaveFolderPanel("Select a folder to save the Car (Remember that it has to be GameLocation/Cars/CollectionFolder, so it can be read for the game)", "", "");
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
		string bundleName = selectedObject.name.ToLower() + ".car";
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
		if (!Directory.Exists(outputPath))
		{
			Directory.CreateDirectory(outputPath);
		}

		AssetBundleBuild bundleBuild = new AssetBundleBuild
		{
			assetBundleName = bundleName,
			assetNames = assetPaths.ToArray(),
		};

		bool success = BuildPipeline.BuildAssetBundles(outputPath, new AssetBundleBuild[] { bundleBuild }, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
		if(success)
		{
			Debug.Log(bundleName + " Exported in: " + outputPath + ". Have Fun :D");
		}
	}
}
