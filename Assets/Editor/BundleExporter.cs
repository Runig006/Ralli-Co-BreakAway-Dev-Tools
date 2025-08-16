using UnityEditor;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;
using UnityEngine.Rendering;





public static class TrackAndCarExporter
{
	private const string LastFolderKeyTrack = "Exporter_LastFolderPath_Track";
	private const string LastFolderKeyCar = "Exporter_LastFolderPath_Car";
	private const string TmpFolderName = "__TrackExportTemp__";
	private const string TmpRoot = "Assets/" + TmpFolderName;

	static readonly HashSet<string> EditorOnlyExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
	{
		".hlsl",
		".shadergraph",
		".shadersubgraph",
		".asmdef",
		".uxml",
		".uss"
	};

	[MenuItem("Assets/Export Track", true)]
	static bool ValidateExportSelectedTrack()
	{
		GameObject o = Selection.activeObject as GameObject;
		return o != null && PrefabUtility.GetPrefabAssetType(o) != PrefabAssetType.NotAPrefab;
	}

	[MenuItem("Assets/Export Track")]
	static void ExportSelectedTrack()
	{
		GameObject go;
		string prefabPath;
		string error;

		if (!TryGetSelectedPrefab(out go, out prefabPath, out error))
		{
			Debug.LogError(error);
			return;
		}

		if (!HasComponentByName(go, "Level"))
		{
			Debug.LogError("Selected prefab does not contain required component 'Level'.");
			return;
		}

		string folderPath;
		if (!TryPickOutputFolder(LastFolderKeyTrack, "Select a folder to save the track (Remember that it has to be GameLocation/Tracks/CollectionFolder, so it can be read for the game)", out folderPath))
		{
			return;
		}

		BuildBundle(prefabPath, go.name, folderPath, ".track");
	}

	[MenuItem("Assets/Export Car", true)]
	static bool ValidateExportSelectedCar()
	{
		GameObject o = Selection.activeObject as GameObject;
		return o != null && PrefabUtility.GetPrefabAssetType(o) != PrefabAssetType.NotAPrefab;
	}

	[MenuItem("Assets/Export Car")]
	static void ExportSelectedCar()
	{
		GameObject go;
		string prefabPath;
		string error;

		if (!TryGetSelectedPrefab(out go, out prefabPath, out error))
		{
			Debug.LogError(error);
			return;
		}

		if (!HasComponentByName(go, "CarParameters"))
		{
			Debug.LogError("Selected prefab does not contain required component 'CarParameters'.");
			return;
		}

		string folderPath;
		if (!TryPickOutputFolder(LastFolderKeyCar, "Select a folder to save the car (Remember that it has to be GameLocation/Cars/CollectionFolder, so it can be read for the game)", out folderPath))
		{
			return;
		}

		BuildBundle(prefabPath, go.name, folderPath, ".car");
	}

	#region Editor Loop
	static bool TryGetSelectedPrefab(out GameObject go, out string prefabPath, out string error)
	{
		error = null;
		go = Selection.activeObject as GameObject;

		if (go == null)
		{
			error = "Please select a prefab first.";
			prefabPath = null;
			return false;
		}

		if (PrefabUtility.GetPrefabAssetType(go) == PrefabAssetType.NotAPrefab)
		{
			error = "The selected object is not a valid prefab.";
			prefabPath = null;
			return false;
		}

		prefabPath = AssetDatabase.GetAssetPath(go);
		if (string.IsNullOrEmpty(prefabPath))
		{
			error = "Could not determine the path of the selected asset.";
			return false;
		}

		return true;
	}

	static bool TryPickOutputFolder(string prefsKey, string title, out string folderPath)
	{
		string lastUsedFolder = EditorPrefs.GetString(prefsKey, "");
		folderPath = EditorUtility.SaveFolderPanel(title, lastUsedFolder, "");

		if (string.IsNullOrEmpty(folderPath))
		{
			return false;
		}

		EditorPrefs.SetString(prefsKey, folderPath);
		return true;
	}

	static bool HasComponentByName(GameObject root, string typeName)
	{
		Component[] comps = root.GetComponentsInChildren<Component>(true);
		for (int i = 0; i < comps.Length; i++)
		{
			Component c = comps[i];
			if (c == null) continue;
			Type t = c.GetType();
			if (t != null && t.Name == typeName) return true;
		}
		return false;
	}

	static void BuildBundle(string prefabPath, string assetName, string outputFolder, string extension)
	{

		string svcPath = null;
		try
		{
			List<string> toBundle = new List<string>();
			toBundle.Add(prefabPath);

			List<string> assetsDependency = GetAssetsFromPrefab(prefabPath);
			toBundle.AddRange(assetsDependency);

			Dictionary<Shader, List<Material>> shaderMaterialCollection = GetShadersFromAssets(assetsDependency);
			foreach (Shader shader in shaderMaterialCollection.Keys)
			{
				string spath = AssetDatabase.GetAssetPath(shader);
				if (string.IsNullOrEmpty(spath)) continue;

				string ext = Path.GetExtension(spath).ToLowerInvariant();
				if (ext == ".shader" || ext == ".compute")
				{
					toBundle.Add(spath);
				}
			}
			ScopedShaderVariantCollector.BeginScope();

			toBundle = toBundle.Distinct().OrderBy(p => p).ToList();

			string bundleName = assetName.ToLower() + extension;
			BuildAssetBundle(toBundle, bundleName, outputFolder);




			//Add ShaderVariants
			if (AssetDatabase.IsValidFolder(TmpRoot) == false)
			{
				AssetDatabase.CreateFolder("Assets", TmpFolderName);
				string svcAssetPath = $"{TmpRoot}/SVC_{System.IO.Path.GetFileNameWithoutExtension(bundleName)}.shadervariants";
				ShaderVariantCollection svc = ScopedShaderVariantCollector.EndScopeAndCreateSvc(svcAssetPath);
				if (svc != null)
				{
					toBundle.Add(svcAssetPath);
					BuildAssetBundle(toBundle, bundleName, outputFolder);
				}
			}
		}
		catch (Exception ex)
		{
			Debug.LogError(string.Format("Failed to export bundle: {0}\n{1}", ex.Message, ex));
		}
		finally
		{
		}


	}
	#endregion

	#region Get the assets
	static List<string> GetAssetsFromPrefab(string prefabPath)
	{
		HashSet<string> filteredDependencies = new HashSet<string>();
		string[] allDeps = AssetDatabase.GetDependencies(prefabPath, true);
		for (int i = 0; i < allDeps.Length; i++)
		{
			string dependency = allDeps[i];
			if (dependency.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			if (IsEditorOnlyPath(dependency))
			{
				continue;
			}
			filteredDependencies.Add(dependency);
		}
		return filteredDependencies.ToList();
	}

	static bool IsEditorOnlyPath(string path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return true;
		}
		if (path.Contains("/Editor/") || path.Contains("\\Editor\\"))
		{
			return true;
		}
		if (path.Contains("/Packages/") || path.Contains("\\Packages\\"))
		{
			return true;
		}

		string ext = Path.GetExtension(path);
		if (!string.IsNullOrEmpty(ext) && EditorOnlyExtensions.Contains(ext))
		{
			return true;
		}
		return false;
	}

	#endregion

	#region Shader Variants
	static Dictionary<Shader, List<Material>> GetShadersFromAssets(List<string> assetPaths)
	{
		Dictionary<Shader, List<Material>> shaderMaterialCollection = new Dictionary<Shader, List<Material>>();

		IEnumerable<string> mats = assetPaths.Where(p => p.EndsWith(".mat", StringComparison.OrdinalIgnoreCase));
		foreach (string matPath in mats)
		{
			Material mat = AssetDatabase.LoadAssetAtPath<Material>(matPath);
			if (mat == null || mat.shader == null)
			{
				continue;
			}

			string shaderPath = AssetDatabase.GetAssetPath(mat.shader);
			if (string.IsNullOrEmpty(shaderPath) || shaderPath.Contains("/Packages/"))
			{
				continue;
			}

			List<Material> list;
			if (!shaderMaterialCollection.TryGetValue(mat.shader, out list))
			{
				list = new List<Material>();
				shaderMaterialCollection[mat.shader] = list;
			}
			list.Add(mat);
		}
		return shaderMaterialCollection;
	}
	#endregion

	#region Build The Pack
	static void BuildAssetBundle(List<string> assetPaths, string bundleName, string outputPath)
	{
		string tempOutputPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
		string sourceBundlePath = Path.Combine(tempOutputPath, bundleName);
		string finalBundlePath = Path.Combine(outputPath, bundleName);

		Directory.CreateDirectory(tempOutputPath);

		AssetBundleBuild bundleBuild = new AssetBundleBuild
		{
			assetBundleName = bundleName,
			assetNames = assetPaths.Distinct().OrderBy(p => p).ToArray(),
		};

		BuildPipeline.BuildAssetBundles(
			tempOutputPath,
			new AssetBundleBuild[] { bundleBuild },
			BuildAssetBundleOptions.None,
			EditorUserBuildSettings.activeBuildTarget
		);

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
	#endregion
}
