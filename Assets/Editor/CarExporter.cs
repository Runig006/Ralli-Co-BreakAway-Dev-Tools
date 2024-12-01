using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class CarExporter : MonoBehaviour
{
	private const string LastFolderKey = "CarExporter_LastFolderPath";

	[MenuItem("Assets/Export Car")]
	static void ExportSelectedPrefab()
	{
		Object selectedObject = Selection.activeObject;
		if (selectedObject == null || !(selectedObject is GameObject))
		{
			Debug.LogError("Please select a Car first");
			return;
		}
		
		// Obtener la última carpeta utilizada
		string lastUsedFolder = EditorPrefs.GetString(LastFolderKey, "");
		string folderPath = EditorUtility.SaveFolderPanel(
			"Select a folder to save the Car (Remember that it has to be GameLocation/Cars/CollectionFolder, so it can be read for the game)", 
			lastUsedFolder, 
			""
		);

		if (string.IsNullOrEmpty(folderPath))
		{
		   return;
		}

		// Guardar la última carpeta utilizada
		EditorPrefs.SetString(LastFolderKey, folderPath);

		// Obtener la ruta del prefab y filtrar las dependencias
		string prefabPath = AssetDatabase.GetAssetPath(selectedObject);
		HashSet<string> dependencies = new HashSet<string>(FilterAssets(prefabPath));  // Usar HashSet para asegurar assets únicos
		dependencies.Add(prefabPath); 
		
		List<string> uniqueAssets = new List<string>(dependencies);
		
		// Crear el asset bundle
		string bundleName = selectedObject.name.ToLower() + ".car";
		BuildCarAssetBundle(uniqueAssets, bundleName, folderPath);
	}
	
	static HashSet<string> FilterAssets(string prefabPath)
	{
		HashSet<string> filteredDependencies = new HashSet<string>();  // HashSet para evitar duplicados
		foreach (var dependency in AssetDatabase.GetDependencies(prefabPath, true))
		{
			// Excluir scripts y archivos de editor/paquetes
			if (dependency.EndsWith(".cs") || dependency.Contains("/Editor/") || dependency.Contains("/Packages/"))
			{
				continue;
			}
			
			filteredDependencies.Add(dependency);
		}
		return filteredDependencies;
	}

	static void BuildCarAssetBundle(List<string> assetPaths, string bundleName, string outputPath)
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
