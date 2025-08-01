using UnityEngine;


public class Vector3Composer : RDRSNode
{
    public enum Vector3Axis { X, Y, Z }

    [SerializeField] private RDRSNode x;
    [SerializeField] private RDRSNode y;
    [SerializeField] private RDRSNode z;

    public override object GetValue()
    {
        float xv = this.ReadFloatComponent(this.x?.GetValue(), Vector3Axis.X);
        float yv = this.ReadFloatComponent(this.y?.GetValue(), Vector3Axis.Y);
        float zv = this.ReadFloatComponent(this.z?.GetValue(), Vector3Axis.Z);

        return new Vector3(xv, yv, zv);
    }

    private float ReadFloatComponent(object value, Vector3Axis axis)
    {
        if(value == null)
        {
            return 0f;
        }
        switch (value)
        {
            case float f:
                return f;

            case int i:
                return i;
            case Vector2 v2:
                switch (axis)
                {
                    case Vector3Axis.X:
                        return v2.x;
                    case Vector3Axis.Y:
                        return v2.y;
                    default:
                        Debug.LogWarning($"[Vector3ComposerReader] {axis} no found in Vector2.");
                        return 0f;
                }
            case Vector3 v3:
                switch (axis)
                {
                    case Vector3Axis.X:
                        return v3.x;
                    case Vector3Axis.Y:
                        return v3.y;
                    case Vector3Axis.Z:
                        return v3.z;
                    default:
                        Debug.LogWarning($"[Vector3ComposerReader] {axis} How?");
                        return 0f;
                }
            default:
                {
                    Debug.LogWarning($"[Vector3ComposerReader] Value not compatible: {value?.GetType().Name ?? "null"}");
                    return 0f;
                }
        }
    }
}
