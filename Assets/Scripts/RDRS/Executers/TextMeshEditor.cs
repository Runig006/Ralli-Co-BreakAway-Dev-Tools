using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextMeshEditor : RDRSExecutorWithFrequency
{
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private RDRSReaderBase[] textTargetReaders;

    private object[] lastInputs;
    private TMP_Text[] cachedTargets;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
    
        TMP_Text[] textTargets = this.GetTargetTexts();
        if (textTargets == null || textTargets.Length == 0)
        {
            return;
        }

        string finalText = value?.ToString() ?? "";

        foreach (TMP_Text tmp in textTargets)
        {
            if (tmp == null)
            {
                continue;
            }
            tmp.text = finalText;
        }
    }

    ///////////////////
    // Get components with Cache
    ///////////////////

    public TMP_Text[] GetTargetTexts()
    {
        if (this.textTargetReaders == null || this.textTargetReaders.Length == 0)
        {
            return System.Array.Empty<TMP_Text>();
        }

        object[] currentInputs = new object[this.textTargetReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.textTargetReaders.Length; i++)
        {
            RDRSReaderBase reader = this.textTargetReaders[i];
            currentInputs[i] = reader != null ? reader.GetValue() : null;

            if (!needsRefresh && (this.lastInputs == null || this.lastInputs.Length != currentInputs.Length || !ReferenceEquals(currentInputs[i], this.lastInputs[i])))
            {
                needsRefresh = true;
            }
        }

        if (!needsRefresh && this.cachedTargets != null)
        {
            return this.cachedTargets;
        }

        this.lastInputs = currentInputs;
        this.cachedTargets = this.ResolveTMPTextsFrom(currentInputs);
        return this.cachedTargets;
    }

    private TMP_Text[] ResolveTMPTextsFrom(object[] inputs)
    {
        List<TMP_Text> collected = new();

        foreach (object result in inputs)
        {
            if (result is TMP_Text[] array)
            {
                collected.AddRange(array);
            }
            else if (result is TMP_Text single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<TMP_Text>());
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is TMP_Text tmp)
                    {
                        collected.Add(tmp);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<TMP_Text>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
