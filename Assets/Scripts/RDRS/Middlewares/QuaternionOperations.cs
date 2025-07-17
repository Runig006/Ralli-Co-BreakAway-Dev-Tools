using System;
using UnityEngine;

public class QuaternionOperations : RDRSReaderBase
{
    public enum QuaternionOperation
    {
        LookRotation,
        FromToRotation,
        Euler,
        AngleAxis, 
        Inverse,
        Multiply
    }

    [SerializeField] private QuaternionOperation operation;

    [SerializeField] private RDRSReaderBase inputA;
    [SerializeField] private RDRSReaderBase inputB;

    public override object GetValue()
    {
        object aRaw = inputA?.GetValue();
        object bRaw = inputB?.GetValue();

        switch (operation)
        {
            case QuaternionOperation.LookRotation:
            {
                Vector3 forward = this.ResolveVector3(aRaw);
                Vector3 up = this.ResolveVector3(bRaw, Vector3.up);
                return Quaternion.LookRotation(forward, up);
            }

            case QuaternionOperation.FromToRotation:
            {
                Vector3 from = this.ResolveVector3(aRaw);
                Vector3 to = this.ResolveVector3(bRaw);
                
                return Quaternion.FromToRotation(from, to);
            }

            case QuaternionOperation.Euler:
            {
                Vector3 euler = this.ResolveVector3(aRaw);
                return Quaternion.Euler(euler);
            }

            case QuaternionOperation.AngleAxis:
            {
                float angle = System.Convert.ToSingle(aRaw);
                Vector3 axis = this.ResolveVector3(bRaw);
                return Quaternion.AngleAxis(angle, axis);
            }

            case QuaternionOperation.Inverse:
            {
                Quaternion q = this.ResolveQuaternion(aRaw);
                return Quaternion.Inverse(q);
            }

            case QuaternionOperation.Multiply:
            {
                Quaternion qa = this.ResolveQuaternion(aRaw);
                Quaternion qb = this.ResolveQuaternion(bRaw);
                return qa * qb;
            }

            default:
                return Quaternion.identity;
        }
    }

    private Vector3 ResolveVector3(object obj, Vector3 fallback = default)
    {
        if (obj is Vector3 v) {
            return v;
        }
        if (obj is Transform t)
        {
            return t.forward;
        }
        if (obj is GameObject go)
        {
            return go.transform.forward;
        }
        return fallback;
    }

    private Quaternion ResolveQuaternion(object obj)
    {
        if (obj is Quaternion q)
        {
            return q;
        }
        if (obj is Transform t)
        {
            return t.rotation;
        }
        if (obj is GameObject go)
        {
            return go.transform.rotation;
        }
        return Quaternion.identity;
    }
}
