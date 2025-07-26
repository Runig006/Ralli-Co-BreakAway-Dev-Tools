using System;
using System.Collections;
using UnityEngine;

public class AccumulatorMiddleware : RDRSReaderWithFrequency
{
    [SerializeField] private RDRSReaderBase valueReader;

    [SerializeField] private float startingValue = 0f;
    [SerializeField] private bool resetOnEnable = false;
    
    [SerializeField] private bool clamp = true;
    [SerializeField] private bool allowOverflowLoop = true;
    [SerializeField] private Vector2 range = new Vector2(0f, 0f);

    [SerializeField] private bool useDeltaTime = true;

    public float accumulated;
    
    public override object GetValue()
    {
        return this.accumulated;
    }

    protected override void OnEnable()
    {
        if (resetOnEnable)
        {
            this.accumulated = this.startingValue;
        }
        base.OnEnable();
    }

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object valueRaw = this.GetExecuteValue();
        float delta;
        if(valueRaw is bool value)
        {
            delta = value ? 1f : -1f;
        }
        delta = System.Convert.ToSingle(valueRaw);

        if (this.useDeltaTime)
        {
            delta *= Time.deltaTime;
        }

        this.accumulated += delta;

        if (this.clamp)
        {
            float length = this.range.y - this.range.x;
            if (length <= 0f)
            {
                Debug.LogWarning("[AccumulatorReader] Invalidad range: range.y has to be greater range.x");
                return;
            }

            if (this.allowOverflowLoop)
            {
                this.accumulated = (this.accumulated - this.range.x) % length;

                if (this.accumulated < 0f)
                {
                    this.accumulated += length;
                }

                this.accumulated += this.range.x;
            }
            else
            {
                this.accumulated = Mathf.Clamp(this.accumulated, this.range.x, this.range.y);
            }
        }
    }
}
