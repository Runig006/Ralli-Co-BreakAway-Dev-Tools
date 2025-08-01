using UnityEngine;

public class Vector3OperationsReader : RDRSNode
{
    public enum Vector3Operations
    {
        Add,
        Subtract,
        Direction,
        RawDirection,
        Negate,
        Return,
    }
    
    public enum TransformVector3Property
    {
        Position,
        LocalPosition,
        RotationEuler,
        LocalRotationEuler,
        Scale,
        LocalScale
    }

    [SerializeField] private RDRSNode inputA;
    [SerializeField] private RDRSNode inputB;

    [SerializeField] private Vector3Operations operation;
    [SerializeField] private TransformVector3Property transformProperty = TransformVector3Property.Position;

    public override object GetValue()
    {
        Vector3 a = this.GetVector3(inputA?.GetValue());
        Vector3 b = this.GetVector3(inputB?.GetValue());

        switch (operation)
        {
            case Vector3Operations.Add:
                return a + b;
            case Vector3Operations.Subtract:
                return a - b;
            case Vector3Operations.Direction:
                return (b - a).normalized;
            case Vector3Operations.RawDirection:
                return b - a;
            case Vector3Operations.Negate:
                return a * -1;
            case Vector3Operations.Return:
                return a;
            default:
                return new Vector3();
        }
    }
    
    private Vector3 GetVector3(object obj)
    {
        if (obj is Vector3 v)
        {
            return v;
        }
        if (obj is Transform t)
        {
            return t.position;
        }
        if (obj is GameObject go)
        {
            return ResolveTransformProperty(go.transform);
        }
        return Vector3.zero;
    }
    
    private Vector3 ResolveTransformProperty(Transform t)
    {
        switch (this.transformProperty)
        {
            case TransformVector3Property.Position:
                return t.position;
            case TransformVector3Property.LocalPosition:
                return t.localPosition;
            case TransformVector3Property.RotationEuler:
                return t.rotation.eulerAngles;
            case TransformVector3Property.LocalRotationEuler:
                return t.localRotation.eulerAngles;
            case TransformVector3Property.Scale:
                return t.lossyScale;
            case TransformVector3Property.LocalScale:
                return t.localScale;
            default:
                return Vector3.zero;
        }
    }
}