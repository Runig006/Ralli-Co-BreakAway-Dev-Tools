using UnityEngine;
using System;
using System.Globalization;

using System.Text.RegularExpressions;
using System.Collections.Generic;


public class StringFormatMiddleware : RDRSNode
{
    private enum FormatMode
    {
        None = 0,
        DateTime = 1
    }

    [SerializeField] private RDRSNode[] sources;
    [SerializeField] private string format = "{0}";

    private Dictionary<int, FormatMode> indexFormats;

    private void Awake()
    {
        this.indexFormats = this.PrecacheFormat();
    }

    private void OnValidate()
    {
        this.indexFormats = this.PrecacheFormat();
    }

    public override object GetValue()
    {
        object[] values = this.GetValuesForString();

        if (this.indexFormats != null && this.indexFormats.Count > 0)
        {
            foreach (KeyValuePair<int, FormatMode> kv in this.indexFormats)
            {
                int idx = kv.Key;
                object val = values[idx];
                double valueTransformed;
                
                if(val == null)
                {
                    continue;
                }

                switch (kv.Value)
                {
                    case FormatMode.DateTime:
                        valueTransformed = Convert.ToSingle(val, CultureInfo.InvariantCulture);
                        values[idx] = DateTime.UnixEpoch.AddSeconds(valueTransformed);
                        break;
                }
            }
        }
        
        try
        {
            return string.Format(CultureInfo.InvariantCulture, format, values);
        }
        catch (FormatException e)
        {
            Debug.LogWarning($"[StringFormatMiddleware] Format error: {e.Message} in format \"{format}\"");
            return "FORMAT_ERR";
        }
    }
    
    private object[] GetValuesForString()
    {
        object[] values = new object[sources.Length];
        for (int i = 0; i < sources.Length; i++)
        {
            RDRSNode reader = sources[i];
            if (reader == null)
            {
                values[i] = "null";
                continue;
            }

            object raw = reader.GetValue();

            if (raw is bool b)
            {
                values[i] = b ? "1" : "0";
            }
            else
            {
                values[i] = raw;
            }
        }
        return values;
    }
    
    private Dictionary<int,FormatMode> PrecacheFormat()
    {
        Dictionary<int, FormatMode> indexFormats = new Dictionary<int, FormatMode>();

        var matches = Regex.Matches(format, @"\{(\d+):([^}]*)\}");
        foreach (Match m in matches)
        {
            int idx = int.Parse(m.Groups[1].Value);
            string fmt = m.Groups[2].Value;
            if (Regex.IsMatch(fmt, "[yMdHhmsfF]"))
            {
                indexFormats[idx] = FormatMode.DateTime;
            }
        }
        return indexFormats;
    }
}
