using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageEditor : RDRSExecutorWithFrequency
{
    public enum ImageEditMode
    {
        Sprite,
        FillAmount,
        Color
    }



    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private RDRSReaderBase[] imageTargetReaders;

    [SerializeField] private ImageEditMode propertyToEdit;

    private object[] lastInputs;
    private Image[] cachedTargets;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
        Image[] imageTargets = this.GetTargetImages();
        if (imageTargets == null || imageTargets.Length == 0)
        {
            return;
        }

        foreach (Image img in imageTargets)
        {
            if (img == null)
            {
                continue;
            }

            switch (this.propertyToEdit)
            {
                case ImageEditMode.Sprite:
                    img.sprite = value as Sprite;
                    break;
                case ImageEditMode.FillAmount:
                    img.fillAmount = Mathf.Clamp01(System.Convert.ToSingle(value));
                    break;
                case ImageEditMode.Color:
                    if (value is Color color)
                    {
                        img.color = color;
                    }
                    else if (value is string colorString && ColorUtility.TryParseHtmlString(colorString, out var parsedColor))
                    {
                        img.color = parsedColor;
                    }
                    
                    break;
            }
        }
    }
    
    ///////////////////
    // Get components with Cache
    ///////////////////

    public Image[] GetTargetImages()
    {
        if (this.imageTargetReaders == null || this.imageTargetReaders.Length == 0)
        {
            return System.Array.Empty<Image>();
        }

        object[] currentInputs = new object[this.imageTargetReaders.Length];
        bool needsRefresh = false;

        for (int i = 0; i < this.imageTargetReaders.Length; i++)
        {
            RDRSReaderBase reader = this.imageTargetReaders[i];
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
        this.cachedTargets = this.ResolveImagesFrom(currentInputs);
        return this.cachedTargets;
    }

    private Image[] ResolveImagesFrom(object[] inputs)
    {
        List<Image> collected = new();

        foreach (object result in inputs)
        {
            if (result is Image[] array)
            {
                collected.AddRange(array);
            }
            else if (result is Image single)
            {
                collected.Add(single);
            }
            else if (result is GameObject go)
            {
                collected.AddRange(go.GetComponentsInChildren<Image>());
            }
            else if (result is Object[] objArray)
            {
                foreach (Object obj in objArray)
                {
                    if (obj is Image img)
                    {
                        collected.Add(img);
                    }
                    else if (obj is GameObject go2)
                    {
                        collected.AddRange(go2.GetComponentsInChildren<Image>());
                    }
                }
            }
        }

        return collected.ToArray();
    }
}
