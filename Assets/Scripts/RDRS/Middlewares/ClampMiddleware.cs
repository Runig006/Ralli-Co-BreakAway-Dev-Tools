using UnityEngine;

public class ClampMiddleware : RDRSNode
{
    [SerializeField] private RDRSNode source;

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
        float value = System.Convert.ToSingle(raw);
        return Mathf.Clamp(value, min, max);
    }
}
