using System;
using UnityEngine;

public class AutomaticReaderFinder : RDRSReaderBase
{
    [SerializeField] private string tagFilter;
    [SerializeField] private RDRSReaderBase typeReference;

    private IRDRSReader resolvedReader;

    private void OnEnable()
    {
        if (this.typeReference == null)
        {
            Debug.LogWarning($"{name}: No reference type set in AutomaticReader.");
            return;
        }

        Type desiredType = this.typeReference.GetType();
        Transform current = transform.parent;

        while (current != null)
        {
            Component[] candidates = current.GetComponents(desiredType);

            foreach (Component candidate in candidates)
            {
                if (candidate is IRDRSReader rdrs)
                {
                    if (string.IsNullOrEmpty(tagFilter) || (candidate is RDRSReaderBase baseReader && baseReader.Tag == tagFilter))
                    {
                        resolvedReader = rdrs;
                        return;
                    }
                }
            }

            current = current.parent;
        }

        Debug.LogWarning($"{name}: No matching reader of type {desiredType} found in parent hierarchy.");
    }

    public override object GetValue()
    {
        return resolvedReader != null ? resolvedReader.GetValue() : null;
    }
}
