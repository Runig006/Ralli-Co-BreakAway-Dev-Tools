using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Build;

[Serializable]
internal sealed class VariantDescJson
{
    public string shader;
    public string pass;
    public string[] keywords;
}

[Serializable]
internal sealed class VariantDBFileJson
{
    public string unity;
    public string platform;
    public string gfxAPIs;
    public string colorSpace;
    public string srp;
    public string srpGuid;
    public string pipelineAssetHash;
    public string dateUtc;
    public VariantDescJson[] variants;
}

internal static class ExporterWhitelist
{
    private const string PrefsKeyPath = "Exporter_PlayerVariants_JsonPath";

    private static bool loaded = false;
    private static readonly HashSet<string> keys = new HashSet<string>(StringComparer.Ordinal);
    private static string info = "(no whitelist loaded)";
    
    public static HashSet<string> GetKeys()
    {
        EnsureLoaded(); 
        return keys;
    }
    
    public static string GetInfo()
    {
        return info;
    }

    [MenuItem("Tools/Exporter/Set Player Variants JSON...")]
    private static void PickJson()
    {
        string start = ProjectRoot();
        string path = EditorUtility.OpenFilePanel("Pick player shader variants JSON", start, "json");
        if (!string.IsNullOrEmpty(path))
        {
            EditorPrefs.SetString(PrefsKeyPath, path);
            loaded = false;
            Debug.Log("[ExporterWhitelist] JSON set: " + path);
        }
    }

    private static void EnsureLoaded()
    {
        if (loaded)
        {
            return;
        }

        keys.Clear();
        string path = EditorPrefs.GetString(PrefsKeyPath, string.Empty);
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            info = "No JSON set/found → stripper idle";
            Debug.LogWarning("[ExporterWhitelist] " + info);
            loaded = false;
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            VariantDBFileJson db = JsonUtility.FromJson<VariantDBFileJson>(json);

            int added = 0;
            if (db != null && db.variants != null)
            {
                for (int i = 0; i < db.variants.Length; i++)
                {
                    VariantDescJson v = db.variants[i];
                    if (v == null)
                    {
                        continue;
                    }
                    if (string.IsNullOrEmpty(v.shader) || string.IsNullOrEmpty(v.pass))
                    {
                        continue;
                    }

                    string[] kws = (v.keywords != null) ? (string[])v.keywords.Clone() : Array.Empty<string>();
                    Array.Sort(kws, StringComparer.Ordinal);

                    string key = v.shader + "|" + v.pass + "|" + string.Join(";", kws);
                    if (keys.Add(key))
                    {
                        added++;
                    }
                }
            }

            info = "OK • variants=" + added + " • path=" + path;
            Debug.Log("[ExporterWhitelist] " + info);
            loaded = true;
        }
        catch (Exception ex)
        {
            info = "Load error → stripper idle";
            Debug.LogError("[ExporterWhitelist] Failed to load JSON:\n" + ex);
            keys.Clear();
        }
    }

    private static string ProjectRoot()
    {
        string assets = Application.dataPath;
        string root = Path.GetDirectoryName(assets);
        return string.IsNullOrEmpty(root) ? "." : root;
    }
}


internal sealed class ExporterShaderStripper : IPreprocessShaders
{
    public int callbackOrder => -2000;

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        HashSet<string> wl = ExporterWhitelist.GetKeys();
        if (wl.Count == 0)
        {
            throw new Exception("The Stripper Shader File is not loaded");
        }

        string shaderName = (shader != null) ? shader.name : "<null>";
        string passName = string.IsNullOrEmpty(snippet.passName) ? snippet.passType.ToString() : snippet.passName;

        for (int i = data.Count - 1; i >= 0; --i)
        {
            ShaderKeyword[] kwsObj = data[i].shaderKeywordSet.GetShaderKeywords();
            int n = (kwsObj != null) ? kwsObj.Length : 0;

            string[] names = new string[n];
            for (int k = 0; k < n; k++)
            {
                names[k] = kwsObj[k].name;
            }
            Array.Sort(names, StringComparer.Ordinal);

            string key = shaderName + "|" + passName + "|" + string.Join(";", names);
            if (wl.Contains(key))
            {
                //Debug.Log($"{key} striped");
                data.RemoveAt(i);
            }
            else
            {
                //Debug.Log($"{key} not striped");
            }
        }
    }
}
