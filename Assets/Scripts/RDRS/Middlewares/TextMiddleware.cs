using UnityEngine;
using System;
using System.Globalization;

public class StringFormatMiddleware : RDRSReaderBase
{
    [SerializeField] private RDRSReaderBase[] sources;
    [SerializeField] private string format = "{0}";

    public override object GetValue()
    {
        object[] values = new object[sources.Length];

        for (int i = 0; i < sources.Length; i++)
        {
            var reader = sources[i];
            if (reader == null)
            {
                values[i] = "null";
                continue;
            }

            object raw = reader.GetValue();

            if (raw is bool b)
            {
                values[i] = b ? "True" : "False";
            }
            else
            {
                try
                {
                    values[i] = Convert.ChangeType(raw, typeof(object), CultureInfo.InvariantCulture);
                }
                catch
                {
                    Debug.LogWarning($"[StringFormatMiddleware] Unsupported value at index {i}: {raw?.GetType().Name ?? "null"}");
                    values[i] = "ERR";
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
}
