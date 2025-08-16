using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSourceEditor : RDRSNodeWithFrequency
{
    public enum AudioProperty
    {
        Volume = 0,
        Pitch = 1,
        PlayState = 2,
        PlayOnce = 3,
    }

    public enum PlayStrategy
    {
        PlayFromStart = 0,
        PlayFromRandom = 1,
        ResumeOrPlay = 2,
    }

    public enum PlayVolumeStrategy
    {
        LeaveVolume = 0,
        FadeIn = 1,
    }

    public enum StopStrategy
    {
        StopImmediate = 0,
        FadeOut = 1
    }

    [SerializeField] private RDRSNode[] audioReaders;
    [SerializeField] private RDRSNode valueReader;
    [SerializeField] private AudioProperty propertyToEdit;
    [SerializeField] private PlayStrategy playStrategy = PlayStrategy.PlayFromStart;
    [SerializeField] private PlayVolumeStrategy playVolumeStrategy = PlayVolumeStrategy.LeaveVolume;
    [SerializeField] private float fadeInDuration = 0.25f;

    [SerializeField] private StopStrategy stopStrategy = StopStrategy.StopImmediate;
    [SerializeField] private float fadeOutDuration = 1f;

    private readonly Dictionary<AudioSource, Coroutine> fadeInCoroutines = new();
    private readonly Dictionary<AudioSource, Coroutine> fadeOutCoroutines = new();
    private readonly Dictionary<AudioSource, float> originalVolumes = new();

    private object[] lastInputs;
    private AudioSource[] cachedSources;


    public override void Execute()
    {
        object value = this.valueReader?.GetValue();

        AudioSource[] targets = GetTargetSources();
        if (targets == null || targets.Length == 0)
        {
            return;
        }

        foreach (AudioSource source in targets)
        {
            if (source == null)
            {
                continue;
            }

            switch (this.propertyToEdit)
            {
                case AudioProperty.Volume:
                    source.volume = Mathf.Clamp01(System.Convert.ToSingle(value));
                    break;

                case AudioProperty.Pitch:
                    source.pitch = System.Convert.ToSingle(value);
                    break;


                case AudioProperty.PlayState:
                    bool enabled = RDRSUtils.toBoolean(value);
                    if (enabled && source.isPlaying == false)
                    {
                        switch (this.playStrategy)
                        {
                            case PlayStrategy.PlayFromStart:
                                source.Stop();
                                source.time = 0f;
                                source.Play();
                                break;

                            case PlayStrategy.PlayFromRandom:
                                float t = Random.Range(0f, source.clip.length);
                                source.Stop();
                                source.time = t;
                                source.Play();
                                break;

                            case PlayStrategy.ResumeOrPlay:
                                source.Play();
                                break;
                        }

                        if (this.playVolumeStrategy == PlayVolumeStrategy.FadeIn)
                        {
                            this.HandleFadeIn(source);
                        }
                        else
                        {
                            this.RestoreOriginalVolume(source);
                        }
                    }
                    else if (enabled == false && source.isPlaying)
                    {
                       
                        switch (this.stopStrategy)
                        {
                            case StopStrategy.StopImmediate:
                                source.Stop();
                                break;

                            case StopStrategy.FadeOut:
                                this.HandleFadeOut(source);
                                break;
                        }
                    }
                    break;
            }
        }
    }

    ///////////////////
    // Fades
    ///////////////////
    
    //In
    private void HandleFadeIn(AudioSource source)
    {
        this.SaveVolumen(source);

        if (this.fadeInCoroutines.ContainsKey(source))
        {
            return;
        }
        
        if (fadeOutCoroutines.TryGetValue(source, out Coroutine fadeOut))
        {
            StopCoroutine(fadeOut);
            this.fadeOutCoroutines.Remove(source);
        }
        else
        {
            source.volume = 0f;
        }
        this.fadeInCoroutines[source] = StartCoroutine(this.FadeInCoroutine(source, this.originalVolumes[source], this.fadeInDuration));
    }

    private IEnumerator FadeInCoroutine(AudioSource source, float targetVolume, float duration)
    {
        float start = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(start, targetVolume, time / duration);
            yield return null;
        }

        source.volume = targetVolume;
        this.fadeInCoroutines.Remove(source);
    }

    //Out
    private void HandleFadeOut(AudioSource source)
    {
        this.SaveVolumen(source);
        if (this.fadeOutCoroutines.ContainsKey(source))
        {
            return;
        }
        if (this.fadeInCoroutines.TryGetValue(source, out Coroutine fadeIn))
        {
            StopCoroutine(fadeIn);
            this.fadeInCoroutines.Remove(source);
        }
        this.fadeOutCoroutines[source] = StartCoroutine(this.FadeOutCoroutine(source, this.fadeOutDuration));
    }

    private IEnumerator FadeOutCoroutine(AudioSource source, float duration)
    {
        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        source.Stop();
        this.fadeOutCoroutines.Remove(source);
    }
    
   
    ///////////////////
    // Volumen
    ///////////////////
    
    private void SaveVolumen(AudioSource source)
    {
        if (this.originalVolumes.ContainsKey(source) == false)
        {
            this.originalVolumes[source] = source.volume;
        }
    }
    
    private void RestoreOriginalVolume(AudioSource source)
    {
        if (originalVolumes.TryGetValue(source, out float vol))
        {
            source.volume = vol;
        }
    }

    ///////////////////
    // Get components with cache
    ///////////////////

    public AudioSource[] GetTargetSources()
    {
        if (this.audioReaders == null || this.audioReaders.Length == 0)
        {
            return System.Array.Empty<AudioSource>();
        }

        object[] currentInputs = new object[this.audioReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.audioReaders.Length; i++)
        {
            RDRSNode reader = this.audioReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedSources != null)
        {
            return this.cachedSources;
        }

        this.lastInputs = currentInputs;
        this.cachedSources = this.ResolveAudioSourcesFrom(currentInputs);
        return this.cachedSources;
    }

    private AudioSource[] ResolveAudioSourcesFrom(object[] inputs)
    {
        List<AudioSource> collected = new();

        foreach (object result in inputs)
        {
            if (result is AudioSource[] array)
            {
                collected.AddRange(array);
            }
            else if (result is AudioSource single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<AudioSource>());
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is AudioSource a)
                    {
                        collected.Add(a);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<AudioSource>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
