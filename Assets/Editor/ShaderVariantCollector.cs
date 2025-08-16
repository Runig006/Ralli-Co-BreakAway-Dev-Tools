using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor.Build;

internal class ScopedShaderVariantCollector : IPreprocessShaders
{
    public int callbackOrder => int.MinValue;

    static bool active;
    static readonly HashSet<VariantKey> collected = new HashSet<VariantKey>();

    struct VariantKey
    {
        public string shaderName;
        public PassType passType;
        public string[] keywordsSorted;

        public override int GetHashCode()
        {
            unchecked
            {
                int h = shaderName?.GetHashCode() ?? 0;
                h = (h * 397) ^ (int)passType;
                for (int i = 0; i < keywordsSorted.Length; i++)
                {
                    h = (h * 397) ^ keywordsSorted[i].GetHashCode();
                }
                return h;
            }
        }
        public override bool Equals(object obj)
        {
            if (!(obj is VariantKey o))
            {
                return false;
            }
            if (passType != o.passType || shaderName != o.shaderName)
            {
                return false;
            }
            if (keywordsSorted.Length != o.keywordsSorted.Length)
            {
                return false;
            }
            for (int i = 0; i < keywordsSorted.Length; i++)
            {
                if (keywordsSorted[i] != o.keywordsSorted[i]) { 
                    return false; 
                }
            }
            return true;
        }
    }

    public void OnProcessShader(Shader shader, ShaderSnippetData snippet, IList<ShaderCompilerData> data)
    {
        if (!active)
        {
            return;
        }
        if (shader == null)
        {
            return;
        }
        if (data == null || data.Count == 0)
        {
            return;
        }

        for (int i = 0; i < data.Count; i++)
        {
            ShaderCompilerData d = data[i];

            ShaderKeyword[] kws = d.shaderKeywordSet.GetShaderKeywords();
            List<string> names = new List<string>(kws.Length);
            for (int k = 0; k < kws.Length; k++)
            {
                names.Add(kws[k].name);;
            }
            names.Sort();

            collected.Add(new VariantKey
            {
                shaderName = shader.name,
                passType = snippet.passType,
                keywordsSorted = names.ToArray()
            });
        }
    }

    public static void BeginScope()
    {
        collected.Clear();
        active = true;
    }

    public static ShaderVariantCollection EndScopeAndCreateSvc(string assetPath)
    {
        active = false;

        if (collected.Count == 0)
        {
            return null;
        }

        ShaderVariantCollection svc = new ShaderVariantCollection { name = System.IO.Path.GetFileNameWithoutExtension(assetPath) };
        int added = 0;

        foreach (VariantKey k in collected)
        {
            Shader shader = Shader.Find(k.shaderName);
            if (shader == null)
            {
                throw new Exception(k.shaderName + " Not Found");
            }
            try
            {
                ShaderVariantCollection.ShaderVariant v = new ShaderVariantCollection.ShaderVariant(shader, k.passType, k.keywordsSorted);
                if (!svc.Contains(v)) { 
                    svc.Add(v); 
                    added++; 
                }
            }
            catch {}
        }

        AssetDatabase.CreateAsset(svc, assetPath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        return svc;
    }
}
