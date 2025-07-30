using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TranslatorMiddleware : RDRSReaderBase
{    
    public enum SupportedType
    {
        Float,
        Color,
        String,
        GameObject,
        AudioClip,
    }

    [Serializable]
    public class TranslatorMiddlewareVariant
    {
        public float floatValue;
        public Color colorValue;
        public string stringValue;
        public GameObject gameObjectValue;
        public AudioClip audioClipValue;

        public object GetValue(SupportedType type)
        {
            switch (type)
            {
                case SupportedType.Float:
                    return this.floatValue;
                case SupportedType.Color:
                    return this.colorValue;
                case SupportedType.String:
                    return this.stringValue;
                case SupportedType.GameObject:
                    return this.gameObjectValue;
                case SupportedType.AudioClip:
                    return this.audioClipValue;
                default:
                    return null;
            };
        }
    }

    [Serializable]
    public class Mapping
    {
        public TranslatorMiddlewareVariant input;
        public TranslatorMiddlewareVariant output;
        public bool useWhenNull;
        public bool defaultKey;
    }


    [SerializeField] private RDRSReaderBase input;

    [SerializeField] private SupportedType inputType;
    [SerializeField] private SupportedType outputType;

    [SerializeField] private bool useRange = false;
    [SerializeField] private bool interpolate = false;
    [SerializeField] [Tooltip("DONT put a default key, or this will not work")] private bool returnValueIfNotFound = false;

    [SerializeField] private List<Mapping> mappings = new();

    public void OnEnable()
    {
        if (inputType == SupportedType.Float)
        {
            this.mappings = this.mappings
                .Where(m => m.input != null && m.input.GetValue(SupportedType.Float) != null)
                .OrderBy(m => (float)m.input.GetValue(SupportedType.Float))
                .ToList();
        }
    }

    public override object GetValue()
    {
        if (input == null)
        {
            return null;
        }

        object raw = input.GetValue();
        if (raw == null)
        {
            Mapping nullFallback = mappings.FirstOrDefault(m => m.useWhenNull);
            if (nullFallback != null)
            {
                return nullFallback.output.GetValue(outputType);
            }
            return null;
        }
        
        foreach (Mapping map in mappings)
        {
            object mapInput = map.input.GetValue(inputType);

            if (mapInput == null)
            {
                continue;
            }

            if (Equals(raw, mapInput))
            {
                return map.output.GetValue(outputType);
            }

            if (    
                this.useRange && 
                this.inputType == SupportedType.Float && 
                mapInput is float mappedFloat && 
                raw is float rawFloat &&
                mappedFloat >= rawFloat
                )
            {
                if (!interpolate)
                {
                    return map.output.GetValue(outputType);
                }
                else
                {
                   
                    int currentIndex = mappings.IndexOf(map);
                    if (currentIndex + 1 < mappings.Count)
                    {
                        Mapping next = mappings[currentIndex + 1];
                        return this.TryInterpolateBetween(map, next, rawFloat);
                    }
                }
            }
        }

        Mapping defaultMapping = mappings.FirstOrDefault(m => m.defaultKey);
        if (defaultMapping != null)
        {
            return defaultMapping.output.GetValue(outputType);
        }
        
        if (this.returnValueIfNotFound)
        {
            return raw;
        }
        
        Debug.LogWarning($"[TranslatorMiddleware] Value not found: {raw}.");
        return null;
    }
    
    private object TryInterpolateBetween(Mapping a, Mapping b, float raw)
    {
        float aInputValue = (float) a.input.GetValue(inputType);
        float bInputValue = (float) b.input.GetValue(inputType);

        float leap = Mathf.InverseLerp(aInputValue, bInputValue, raw);
        if (outputType == SupportedType.Float)
        {
            float aOutput = (float)a.output.GetValue(SupportedType.Float);
            float bOutput = (float)b.output.GetValue(SupportedType.Float);
            float result = Mathf.Lerp(aOutput, bOutput, leap);
            return result;
        }

        if (outputType == SupportedType.Color)
        {
            Color aOutput = (Color)a.output.GetValue(SupportedType.Color);
            Color bOutput = (Color)b.output.GetValue(SupportedType.Color);
            Color result = Color.Lerp(aOutput, bOutput, leap);
            return result;
        }
        Debug.LogWarning($"[TranslatorMiddleware] Interpolation not supported for output type: {outputType}.");
        return null;
    }
}
