using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemEditor : RDRSNodeWithFrequency
{
    public enum ParticleProperty
    {
        EmissionRateOverTime = 0,
        EmissionRateOverDistance = 1,
        StartSize = 2,
        StartSpeed = 3,
        Enable = 4,
        StartColor = 5,
        BurstCount = 6,
        BurstCountRange = 7,
        BurstProbability = 8,
    }

    [SerializeField] private RDRSNode valueReader;
    [SerializeField] private RDRSNode[] particleSystemReaders;
    [SerializeField] private ParticleProperty propertyToEdit;
    [SerializeField] private Vector2Int burstRange = new Vector2Int(0, 0);

    private object[] lastInputs;
    private ParticleSystem[] cachedSystems;

    public override void Execute()
    {
        object value = this.valueReader?.GetValue();

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

            //This switch need the {} for the var emission
            switch (this.propertyToEdit)
            {
                case ParticleProperty.EmissionRateOverTime:
                    {
                        float convert = System.Convert.ToSingle(value);
                        ParticleSystem.EmissionModule emission = ps.emission;
                        ParticleSystem.MinMaxCurve rate = emission.rateOverTime;
                        rate.constant = convert;
                        emission.rateOverTime = rate;
                        break;
                    }

                case ParticleProperty.EmissionRateOverDistance:
                    {
                        float convert = System.Convert.ToSingle(value);
                        ParticleSystem.EmissionModule emission = ps.emission;
                        ParticleSystem.MinMaxCurve overDistance = emission.rateOverDistance;
                        overDistance.constant = convert;
                        emission.rateOverDistance = overDistance;
                        break;
                    }

                case ParticleProperty.StartSize:
                    {
                        float convert = System.Convert.ToSingle(value);
                        ParticleSystem.MainModule main = ps.main;
                        ParticleSystem.MinMaxCurve curve = main.startSize;
                        curve.constant = convert;
                        main.startSize = curve;
                        break;
                    }

                case ParticleProperty.StartSpeed:
                    {
                        float convert = System.Convert.ToSingle(value);
                        ParticleSystem.MainModule main = ps.main;
                        ParticleSystem.MinMaxCurve curve = main.startSpeed;
                        curve.constant = convert;
                        main.startSpeed = curve;
                        break;
                    }

                case ParticleProperty.Enable:
                    {
                        bool enabled = RDRSUtils.toBoolean(value);
                        if (enabled && !ps.isPlaying)
                        {
                            ps.Play();
                        }
                        else if (!enabled && ps.isPlaying)
                        {
                            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
                        }
                        break;
                    }

                case ParticleProperty.StartColor:
                    {
                        if (value is Color color)
                        {
                            ParticleSystem.MainModule main = ps.main;
                            main.startColor = color;
                        }
                        else if (value is string colorString && ColorUtility.TryParseHtmlString(colorString, out var parsedColor))
                        {
                            ParticleSystem.MainModule main = ps.main;
                            main.startColor = parsedColor;
                        }
                        else
                        {
                            Debug.LogWarning("[ParticleSystemEditor] Value not valid for startColor");
                        }
                        break;
                    }
                case ParticleProperty.BurstCount:
                    {
                        int countValue = System.Convert.ToInt32(value);
                        ParticleSystem.EmissionModule emission = ps.emission;
                        int burstCount = emission.burstCount;
                        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[burstCount];
                        emission.GetBursts(bursts);

                        for (int i = 0; i < bursts.Length; i++)
                        {
                            if (i >= burstRange.x && i <= burstRange.y)
                            {
                                ParticleSystem.Burst burst = bursts[i];
                                burst.count = new ParticleSystem.MinMaxCurve(countValue);
                                bursts[i] = burst;
                            }
                        }

                        emission.SetBursts(bursts);
                        break;
                    }

                case ParticleProperty.BurstProbability:
                    {
                        float prob = Mathf.Clamp01(System.Convert.ToSingle(value));
                        ParticleSystem.EmissionModule emission = ps.emission;
                        int burstCount = emission.burstCount;
                        ParticleSystem.Burst[] bursts = new ParticleSystem.Burst[burstCount];
                        emission.GetBursts(bursts);

                        for (int i = 0; i < bursts.Length; i++)
                        {
                            if (i >= burstRange.x && i <= burstRange.y)
                            {
                                ParticleSystem.Burst burst = bursts[i];
                                burst.probability = prob;
                                bursts[i] = burst;
                            }
                        }

                        emission.SetBursts(bursts);
                        break;
                    }
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
            RDRSNode reader = particleSystemReaders[i];
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
