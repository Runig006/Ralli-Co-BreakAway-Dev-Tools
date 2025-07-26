using System.Collections.Generic;
using UnityEngine;

public class TrailRendererEditor : RDRSExecutorWithFrequency
{
    public enum TrailProperty
    {
        Time,
        StartWidth,
        EndWidth,
        Emitting
    }

    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private RDRSReaderBase[] trailRendererReaders;
    [SerializeField] private TrailProperty propertyToEdit;

    private object[] lastInputs;
    private TrailRenderer[] cachedRenderers;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
    
        TrailRenderer[] targets = GetTargetRenderers();
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        foreach (TrailRenderer trail in targets)
        {
            if (trail == null)
            {
                continue;
            }

            switch (this.propertyToEdit)
            {
                case TrailProperty.Time:
                    trail.time = System.Convert.ToSingle(value);
                    break;
                case TrailProperty.StartWidth:
                    trail.startWidth = System.Convert.ToSingle(value);
                    break;

                case TrailProperty.EndWidth:
                    trail.endWidth = System.Convert.ToSingle(value);
                    break;
                case TrailProperty.Emitting:
                    if (value is bool b)
                    {
                        trail.emitting = b;
                    }
                    else
                    {
                        float f = System.Convert.ToSingle(value);
                        trail.emitting = f > 0.0001f;
                    }
                    break;
            }
        }
    }

    ///////////////////
    // Get components with Cache
    ///////////////////

    public TrailRenderer[] GetTargetRenderers()
    {
        if (this.trailRendererReaders == null || this.trailRendererReaders.Length == 0)
        {
            return System.Array.Empty<TrailRenderer>();
        }

        object[] currentInputs = new object[this.trailRendererReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.trailRendererReaders.Length; i++)
        {
            RDRSReaderBase reader = this.trailRendererReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedRenderers != null)
        {
            return this.cachedRenderers;
        }

        this.lastInputs = currentInputs;
        this.cachedRenderers = this.ResolveTrailRenderersFrom(currentInputs);
        return this.cachedRenderers;
    }

    private TrailRenderer[] ResolveTrailRenderersFrom(object[] inputs)
    {
        List<TrailRenderer> collected = new();

        foreach (object result in inputs)
        {
            if (result is TrailRenderer[] array)
            {
                collected.AddRange(array);
            }
            else if (result is TrailRenderer single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<TrailRenderer>());
            }
            else if (result is Object[] objArray)
            {
                foreach (var obj in objArray)
                {
                    if (obj is TrailRenderer tr)
                    {
                        collected.Add(tr);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<TrailRenderer>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
