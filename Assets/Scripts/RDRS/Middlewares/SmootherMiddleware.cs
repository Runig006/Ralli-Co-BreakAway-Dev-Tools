using UnityEngine;

public class SmootherMiddleware : RDRSNodeWithFrequency
{
    [SerializeField] private RDRSNode valueReader;

    [SerializeField] private float startingValue = 0f;
    [SerializeField] private bool resetOnEnable = false;
    [SerializeField] private float smoothTime = 0.2f;
    
    [SerializeField] private bool useDeltaTime = true;

    private float currentValue;
    private float velocity;

    public override object GetValue()
    {
        return this.currentValue;
    }

    protected override void OnEnable()
    {
        if (resetOnEnable)
        {
            this.currentValue = this.startingValue;
        }
        base.OnEnable();
    }
    

    public override void Execute()
    {
        object valueRaw = this.valueReader?.GetValue();
        float value = System.Convert.ToSingle(valueRaw);
        if (this.useDeltaTime)
        {
            #if UNITY_EDITOR
            if(this.frequency > 0f)
            {
                Debug.LogWarning("[SmootherReader] You are using deltaTime but frequency is not 0. The result may be incorrect.");
            }
            #endif
           this.currentValue = Mathf.SmoothDamp(this.currentValue, value, ref this.velocity, this.smoothTime, Mathf.Infinity, Time.deltaTime);
        }
        else
        {
            #if UNITY_EDITOR
            if(this.frequency == 0f)
            {
                Debug.LogWarning("[SmootherReader] You are NOT using deltaTime but frequency is 0. The result WILL be incorrect.");
            }
            #endif
            float lerpFactor = this.frequency / (this.smoothTime + Mathf.Epsilon);
            lerpFactor = Mathf.Clamp01(lerpFactor);
            this.currentValue = Mathf.Lerp(this.currentValue, value, lerpFactor);
        }
    }
}
