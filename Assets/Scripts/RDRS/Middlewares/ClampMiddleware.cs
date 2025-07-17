using UnityEngine;

public class ClampMiddleware : RDRSReaderBase
{
    [SerializeField] private RDRSReaderBase source;

    [SerializeField] private float min = 0f;
    [SerializeField] private float max = 1f;

    public override object GetValue()
    {
        if (this.source == null)
        {
            Debug.LogWarning("[ClampMiddleware] Source is null.");
            return 0f;
        }

        object raw = source.GetValue();
        if (raw is not float value)
        {
            Debug.LogWarning($"[ClampMiddleware] Expected float, got {raw?.GetType().Name ?? "null"}");
            return 0f;
        }

        return Mathf.Clamp(value, min, max);
    }
}
