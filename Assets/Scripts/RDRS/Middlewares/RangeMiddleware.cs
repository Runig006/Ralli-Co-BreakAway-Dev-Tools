using UnityEngine;

public class RangeMiddleware : RDRSReaderBase
{
    [SerializeField] private RDRSReaderBase inputSource;

    [SerializeField] private Vector2 inputRange;
    [SerializeField] private Vector2 outputRange;

    public override object GetValue()
    {
        if (this.inputSource == null)
        {
            Debug.LogWarning("[RangeRemapper] Missing input source");
            return 0f;
        }

        object raw = this.inputSource.GetValue();
        float value = System.Convert.ToSingle(raw);

        float t = Mathf.InverseLerp(this.inputRange.x, this.inputRange.y, value);
        return Mathf.Lerp(this.outputRange.x, this.outputRange.y, t);
    }
}
