using System.Collections.Generic;
using UnityEngine;

public class MaterialEditor : RDRSExecutorWithFrequency
{
    public enum MaterialPropertyType
    {
        Float,
        Color,
        Int,
        Vector,
        Keyword
    }

    [SerializeField] private RDRSReaderBase[] materialReaders;
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private string propertyName = "_Color";
    [SerializeField] private MaterialPropertyType propertyType = MaterialPropertyType.Color;

    private object[] lastInputs;
    private Material[] cachedMaterials;

    public override object? GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute(object? value)
    {
        Material[] materials = this.GetTargetMaterials();
        if (materials == null || materials.Length == 0)
        {
            return;
        }

        for (int i = 0; i < materials.Length; i++)
        {
            Material mat = materials[i];
            if (mat == null)
            {
                continue;
            }

            switch (this.propertyType)
            {
                case MaterialPropertyType.Float:
                    if (value is float f)
                    {
                        mat.SetFloat(this.propertyName, f);
                    }
                    break;

                case MaterialPropertyType.Color:
                    if (value is Color c)
                    {
                        mat.SetColor(this.propertyName, c);
                    }
                    else if (value is string s && ColorUtility.TryParseHtmlString(s, out Color parsed))
                    {
                        mat.SetColor(this.propertyName, parsed);
                    }
                    break;

                case MaterialPropertyType.Int:
                    if (value is int iValue)
                    {
                        mat.SetInt(this.propertyName, iValue);
                    }
                    else if (value is float fInt)
                    {
                        mat.SetInt(this.propertyName, Mathf.RoundToInt(fInt));
                    }
                    break;

                case MaterialPropertyType.Vector:
                    if (value is Vector4 v)
                    {
                        mat.SetVector(this.propertyName, v);
                    }
                    break;

                case MaterialPropertyType.Keyword:
                    if (value is bool b)
                    {
                        enabled = b;
                    }
                    else if (value is float fl)
                    {
                        enabled = fl > 0.0001f;
                    }
                    else if (value is int inte)
                    {
                        enabled = inte != 0;
                    }

                    if (enabled)
                    {
                        mat.EnableKeyword(this.propertyName);
                    }
                    else
                    {
                        mat.DisableKeyword(this.propertyName);
                    }
                    break;
            }
        }
    }

    public Material[] GetTargetMaterials()
    {
        if (this.materialReaders == null || this.materialReaders.Length == 0)
        {
            return System.Array.Empty<Material>();
        }

        object[] currentInputs = new object[this.materialReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.materialReaders.Length; i++)
        {
            RDRSReaderBase reader = this.materialReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedMaterials != null)
        {
            return this.cachedMaterials;
        }

        this.lastInputs = currentInputs;
        this.cachedMaterials = this.ResolveMaterialsFrom(currentInputs);
        return this.cachedMaterials;
    }

    private Material[] ResolveMaterialsFrom(object[] inputs)
    {
        List<Material> collected = new List<Material>();

        foreach (object result in inputs)
        {
            if (result is Material mat)
            {
                collected.Add(mat);
            }
            else if (result is Material[] mats)
            {
                collected.AddRange(mats);
            }
            else if (result is Renderer renderer)
            {
                collected.AddRange(renderer.materials);
            }
            else if (result is GameObject go)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    collected.AddRange(r.materials);
                }
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is Material m)
                    {
                        collected.Add(m);
                    }
                    else if (obj is Renderer r)
                    {
                        collected.AddRange(r.materials);
                    }
                    else if (obj is GameObject g)
                    {
                        Renderer r2 = g.GetComponent<Renderer>();
                        if (r2 != null)
                        {
                            collected.AddRange(r2.materials);
                        }
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
