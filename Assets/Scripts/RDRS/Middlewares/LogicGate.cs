using System;
using System.Collections.Generic;
using UnityEngine;

public class LogicGate : RDRSNode
{
    public enum LogicMode
    {
        AllTrue,
        AnyTrue,
        NoneTrue,
        Majority,
        InvertAll,
        Xor
    }

    [SerializeField] private RDRSNode[] conditions;
    [SerializeField] private RDRSNode outputTrue;
    [SerializeField] private RDRSNode outputFalse;
    [SerializeField] private LogicMode mode = LogicMode.AllTrue;

    public override object GetValue()
    {
        int total = 0;
        int passed = 0;

        foreach (RDRSNode cond in conditions)
        {
            if (cond == null)
            {
                continue;
            }
            total++;
            if (this.EvaluateCondition(cond))
            {
                passed++;
            }
        }

        bool result = false;

        switch (mode)
        {
            case LogicMode.AllTrue:
                result = passed == total && total > 0;
                break;
            case LogicMode.AnyTrue:
                result = passed > 0;
                break;
            case LogicMode.NoneTrue:
                result = passed == 0 && total > 0;
                break;
            case LogicMode.Majority:
                result = passed > total / 2;
                break;
            case LogicMode.InvertAll:
                result = passed == 0 && total > 0;
                break;
            case LogicMode.Xor:
                result = passed == 1;
                break;
            default:
                result = false;
                break;
        }
        
        if(result)
        {
            return this.outputTrue ? this.outputTrue.GetValue() : true;
        }else
        {
            return this.outputFalse ? this.outputFalse.GetValue() : false;
        }
    }

    private bool EvaluateCondition(RDRSNode reader)
    {
        if (reader == null)
        {
            return false;
        }

        object val = reader.GetValue();

        if (val == null)
        {
            return false;
        }

        if (val is bool b)
        {
            return b;
        }

        if (val is string s)
        {
            if (bool.TryParse(s, out bool parsedBool))
            {
                return parsedBool;
            }

            if (float.TryParse(s, out float parsedFloat))
            {
                return parsedFloat != 0f;
            }
        }

        try
        {
            float number = Convert.ToSingle(val);
            return number > 0f;
        }
        catch
        {
            // If it isn't a number...but it isnt null, tecnically...is true
            return true;
        }
    }
}
