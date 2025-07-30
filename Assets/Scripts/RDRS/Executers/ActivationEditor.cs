using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class ActivationEditor : RDRSExecutorWithFrequency
{
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private RDRSReaderBase[] targetsReaders;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        bool enable = RDRSUtils.toBoolean(this.GetExecuteValue());
        foreach (object target in this.GetTargets())
        {
            if (target == null)
            {
                continue;
            }
            
            if (target is GameObject go)
            {
                go.SetActive(enable);
            }
            else if (target is Component comp)
            {
                System.Type type = comp.GetType();
                PropertyInfo prop = type.GetProperty("enabled");

                if (prop != null && prop.PropertyType == typeof(bool) && prop.CanWrite)
                {
                    prop.SetValue(comp, enable);
                }
                else
                {
                    comp.gameObject.SetActive(enable);
                }
            }
        }
    }

    private Object[] GetTargets()
    {
        List<Object> collected = new List<Object>();

        foreach (RDRSReaderBase reader in this.targetsReaders)
        {
            if (reader == null)
            {
                continue;
            }

            object result = reader.GetValue();

            if (result is GameObject go)
            {
                collected.Add(go);
            }
            else if (result is GameObject[] goArray)
            {
                collected.AddRange(goArray);
            }
            else if (result is Component comp)
            {
                collected.Add(comp);
            }
            else if (result is Component[] compArray)
            {
                collected.AddRange(compArray);
            }
        }

        return collected.ToArray();
    }
}
