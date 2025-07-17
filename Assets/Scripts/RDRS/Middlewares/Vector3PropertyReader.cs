using UnityEngine;

public class Vector3PropertyReader : RDRSReaderBase
{
    public enum Vector3PropertyType
    {
        Magnitude,
        SqrMagnitude,
        X,
        Y,
        Z,
        MaxComponent,
        MinComponent,
        Sum
    }

    [SerializeField] private RDRSReaderBase input;
    [SerializeField] private Vector3PropertyType property;

    public override object GetValue()
    {
        if (this.input == null)
        {
            return 0f;
        }

        object raw = this.input.GetValue();
        Vector3 v = raw is Vector3 vec ? vec : Vector3.zero;

        switch (this.property)
        {
            case Vector3PropertyType.Magnitude:
                return v.magnitude;
            case Vector3PropertyType.SqrMagnitude:
                return v.sqrMagnitude;
            case Vector3PropertyType.X:
                return v.x;
            case Vector3PropertyType.Y:
                return v.y;
            case Vector3PropertyType.Z:
                return v.z;
            case Vector3PropertyType.MaxComponent:
                return Mathf.Max(v.x, Mathf.Max(v.y, v.z));
            case Vector3PropertyType.MinComponent:
                return Mathf.Min(v.x, Mathf.Min(v.y, v.z));
            case Vector3PropertyType.Sum:
                return v.x + v.y + v.z;
            default:
                return 0f;
        }
    }
}
