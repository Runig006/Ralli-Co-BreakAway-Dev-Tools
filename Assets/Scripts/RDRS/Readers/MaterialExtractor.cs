using System.Collections.Generic;
using UnityEngine;

public class MaterialExtractor : RDRSNode
{
    [SerializeField] private RDRSNode[] sourceReaders;
    [SerializeField] private int[] materialIndices = new int[] { 0 };

    private object[] lastInputs;
    private Material[] cachedMaterials;

    public override object GetValue()
    {
        if (this.sourceReaders == null || this.sourceReaders.Length == 0)
        {
            return null;
        }

        if (this.materialIndices == null || this.materialIndices.Length == 0)
        {
            return null;
        }

        object[] currentInputs = new object[this.sourceReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.sourceReaders.Length; i++)
        {
            RDRSNode reader = this.sourceReaders[i];
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
            if (result is Renderer renderer)
            {
                collected.AddRange(this.GetMaterialsFromRenderer(renderer));
            }
            else if (result is GameObject go)
            {
                Renderer r = go.GetComponent<Renderer>();
                if (r != null)
                {
                    collected.AddRange(this.GetMaterialsFromRenderer(r));
                }
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is Renderer r2)
                    {
                        collected.AddRange(this.GetMaterialsFromRenderer(r2));
                    }
                    else if (obj is GameObject g)
                    {
                        Renderer r3 = g.GetComponent<Renderer>();
                        if (r3 != null)
                        {
                            collected.AddRange(this.GetMaterialsFromRenderer(r3));
                        }
                    }
                }
            }
        }
        return collected.ToArray();
    }


    private Material[] GetMaterialsFromRenderer(Renderer renderer)
    {
        Material[] materials = renderer.materials;
        List<Material> selected = new List<Material>();

        for (int i = 0; i < this.materialIndices.Length; i++)
        {
            int index = this.materialIndices[i];
            if (index >= 0 && index < materials.Length)
            {
                selected.Add(materials[index]);
            }
        }

        return selected.ToArray();
    }
}
