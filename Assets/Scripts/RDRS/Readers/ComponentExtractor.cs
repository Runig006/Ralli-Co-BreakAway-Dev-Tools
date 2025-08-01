using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentExtractor : RDRSNode
{
    public enum CollectableComponentType
    {
        AudioSource,
        ParticleSystem,
        Transform,
        Light,
        Renderer
    }

    [SerializeField] private RDRSNode[] gameObjectReaders;
    [SerializeField] private CollectableComponentType componentType = CollectableComponentType.AudioSource;

    private object[] lastInputs;
    private Component[] cachedComponents;


    public override object GetValue()
    {
        if (this.gameObjectReaders == null || this.gameObjectReaders.Length == 0)
        {
            return Array.Empty<Component>();
        }

        object[] currentInputs = new object[this.gameObjectReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.gameObjectReaders.Length; i++)
        {
            RDRSNode reader = this.gameObjectReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedComponents != null)
        {
            return this.cachedComponents;
        }

        this.lastInputs = currentInputs;
        this.cachedComponents = this.ResolveComponentsFrom(currentInputs);
        return this.cachedComponents;
    }

    private Component[] ResolveComponentsFrom(object[] inputs)
    {
        List<GameObject> collectedObjects = new();

        foreach (object result in inputs)
        {
            if (result == null)
            {
                continue;
            }

            switch (result)
            {
                case GameObject go:
                    collectedObjects.Add(go);
                    break;

                case Component comp:
                    collectedObjects.Add(comp.gameObject);
                    break;

                case IEnumerable enumerable:
                    foreach (var item in enumerable)
                    {
                        if (item is GameObject go2)
                        {
                            collectedObjects.Add(go2);
                        }
                        else if (item is Component comp2)
                        {
                            collectedObjects.Add(comp2.gameObject);
                        }
                    }
                    break;

                default:
                    Debug.LogWarning($"[ComponentExtractor] Unsupported value type: {result.GetType().Name}");
                    break;
            }
        }

        if (collectedObjects.Count == 0)
        {
            return Array.Empty<Component>();
        }

        Type targetType = GetTypeFromEnum(this.componentType);
        if (targetType == null)
        {
            Debug.LogError($"[ComponentExtractor] Invalid component type: {componentType}");
            return Array.Empty<Component>();
        }

        List<Component> resultComponents = new();

        foreach (GameObject go in collectedObjects)
        {
            if (go == null)
            {
                continue;
            }
            
            Component[] comps = go.GetComponentsInChildren(targetType);
            if (comps != null && comps.Length > 0)
            {
                resultComponents.AddRange(comps);
            }
        }

        return resultComponents.ToArray();
    }

    private Type GetTypeFromEnum(CollectableComponentType typeEnum)
    {
        return typeEnum switch
        {
            CollectableComponentType.AudioSource => typeof(AudioSource),
            CollectableComponentType.ParticleSystem => typeof(ParticleSystem),
            CollectableComponentType.Transform => typeof(Transform),
            CollectableComponentType.Light => typeof(Light),
            CollectableComponentType.Renderer => typeof(Renderer),
            _ => null
        };
    }
}
