using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemEditor : RDRSExecutorWithFrequency
{
    public enum ParticleProperty
    {
        EmissionRateOverTime,
        EmissionRateOverDistance,
        StartSize,
        StartSpeed,
        Enable,
        StartColor
    }


    [SerializeField] private RDRSReaderBase[] particleSystemReaders;
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private ParticleProperty propertyToEdit;

    private object[] lastInputs;
    private ParticleSystem[] cachedSystems;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
        
        ParticleSystem[] particleSystemsToEdit = this.GetTargetSystems();
        if (particleSystemsToEdit == null || particleSystemsToEdit.Length == 0)
        {
            return;
        }

        foreach (var ps in particleSystemsToEdit)
        {
            if (ps == null)
            {
                continue;
            }

            switch (this.propertyToEdit)
            {
                case ParticleProperty.EmissionRateOverTime:
                    {
                        float convert = System.Convert.ToSingle(value);
                        var emission = ps.emission;
                        var rate = emission.rateOverTime;
                        rate.constant = convert;
                        emission.rateOverTime = rate;
                        break;
                    }

                case ParticleProperty.EmissionRateOverDistance:
                    {
                        float convert = System.Convert.ToSingle(value);
                        var emission = ps.emission;
                        var overDistance = emission.rateOverDistance;
                        overDistance.constant = convert;
                        emission.rateOverDistance = overDistance;
                        break;
                    }

                case ParticleProperty.StartSize:
                    {
                        float convert = System.Convert.ToSingle(value);
                        var main = ps.main;
                        var curve = main.startSize;
                        curve.constant = convert;
                        main.startSize = curve;
                        break;
                    }

                case ParticleProperty.StartSpeed:
                    {
                        float convert = System.Convert.ToSingle(value);
                        var main = ps.main;
                        var curve = main.startSpeed;
                        curve.constant = convert;
                        main.startSpeed = curve;
                        break;
                    }

                case ParticleProperty.Enable:
                    bool enabled = false;
                    switch (value)
                    {
                        case bool b:
                            enabled = b;
                            break;

                        case float f:
                            enabled = Mathf.Abs(f) > 0.0001f;
                            break;

                        case int i:
                            enabled = i != 0;
                            break;
                    }
                    if (enabled && !ps.isPlaying)
                    {
                        ps.Play();
                    }
                    else if (!enabled && ps.isPlaying)
                    {
                        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                    }
                    break;

                case ParticleProperty.StartColor:
                    if (value is Color color)
                    {
                        var main = ps.main;
                        main.startColor = color;
                    }
                    else if (value is string colorString && ColorUtility.TryParseHtmlString(colorString, out var parsedColor))
                    {
                        var main = ps.main;
                        main.startColor = parsedColor;
                    }
                    else
                    {
                        Debug.LogWarning("[ParticleSystemEditor] Value not valid for startColor");
                    }
                    break;
            }
        }
    }

    ///////////////////
    // Get components with Cache
    ///////////////////

    public ParticleSystem[] GetTargetSystems()
    {
        if (particleSystemReaders == null || particleSystemReaders.Length == 0)
        {
            return System.Array.Empty<ParticleSystem>();
        }

        object[] currentInputs = new object[particleSystemReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < particleSystemReaders.Length; i++)
        {
            RDRSReaderBase reader = particleSystemReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedSystems != null)
        {
            return this.cachedSystems;
        }

        this.lastInputs = currentInputs;
        this.cachedSystems = this.ResolveParticleSystemsFrom(currentInputs);
        return this.cachedSystems;
    }

    private ParticleSystem[] ResolveParticleSystemsFrom(object[] inputs)
    {
        List<ParticleSystem> collected = new();

        foreach (object result in inputs)
        {
            if (result is ParticleSystem[] array)
            {
                collected.AddRange(array);
            }
            else if (result is ParticleSystem single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<ParticleSystem>());
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is ParticleSystem ps)
                    {
                        collected.Add(ps);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<ParticleSystem>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
