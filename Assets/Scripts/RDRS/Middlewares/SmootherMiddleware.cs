using UnityEngine;

public class SmootherMiddleware : RDRSReaderWithFrequency
{
    [SerializeField] private RDRSReaderBase valueReader;

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

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
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
        object valueRaw = this.GetExecuteValue();
        if (valueRaw is not float target)
        {
            Debug.LogWarning($"[SmootherReader] Expected float from source, got: {valueRaw?.GetType().Name ?? "null"}");
            return;
        }
        if (this.useDeltaTime)
        {
            #if UNITY_EDITOR
            if(this.frequency > 0f)
            {
                Debug.LogWarning("[SmootherReader] You are using deltaTime but frequency is not 0. The result may be incorrect.");
            }
            #endif
           this.currentValue = Mathf.SmoothDamp(this.currentValue, target, ref this.velocity, this.smoothTime, Mathf.Infinity, Time.deltaTime);
        }
        else
        {
            this.currentValue = Mathf.Lerp(this.currentValue, target, this.smoothTime);
        }
    }
}
