using System.Collections.Generic;
using UnityEngine;

public class LightEditor : RDRSExecutorWithFrequency
{
    public enum LightProperty
    {
        Intensity,
        Range,
        Color,
        Enabled
    }

    [SerializeField] private RDRSReaderBase[] lightReaders;
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private LightProperty propertyToEdit;

    private object[] lastInputs;
    private Light[] cachedLights;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
        
        Light[] targets = GetTargetLights();
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        foreach (Light light in targets)
        {
            if (light == null)
            {
                continue;
            }

            switch (this.propertyToEdit)
            {
                case LightProperty.Intensity:
                    light.intensity = System.Convert.ToSingle(value);
                    break;

                case LightProperty.Range:
                    light.range =  System.Convert.ToSingle(value);
                    break;

                case LightProperty.Color:
                    if (value is Color color)
                    {
                        light.color = color;
                    }
                    else if (value is string colorString && ColorUtility.TryParseHtmlString(colorString, out var parsedColor))
                    {
                        light.color = parsedColor;
                    }
                    else
                    {
                        Debug.LogWarning("[LightEditor] Color not valid");
                    }
                    break;

                case LightProperty.Enabled:
                    bool enabled = false;
                    if (value is bool b)
                    {
                        enabled = b;
                    }
                    else
                    {
                        float f = System.Convert.ToSingle(value);
                        enabled = f > 0.0001f;
                    }
                    light.enabled = enabled;
                    break;
            }
        }
    }
    
    ///////////////////
    // Get components with Cache
    ///////////////////

    public Light[] GetTargetLights()
    {
        if (this.lightReaders == null || this.lightReaders.Length == 0)
        {
            return System.Array.Empty<Light>();
        }

        object[] currentInputs = new object[this.lightReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.lightReaders.Length; i++)
        {
            RDRSReaderBase reader = this.lightReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedLights != null)
        {
            return this.cachedLights;
        }

        this.lastInputs = currentInputs;
        this.cachedLights = this.ResolveLightsFrom(currentInputs);
        return this.cachedLights;
    }

    private Light[] ResolveLightsFrom(object[] inputs)
    {
        List<Light> collected = new();

        foreach (object result in inputs)
        {
            if (result is Light[] array)
            {
                collected.AddRange(array);
            }
            else if (result is Light single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<Light>());
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is Light l)
                    {
                        collected.Add(l);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<Light>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
