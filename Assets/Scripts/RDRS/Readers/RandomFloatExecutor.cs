using UnityEngine;

public class RandomFloatExecutor : RDRSNodeWithFrequency
{
    [SerializeField] private Vector2 range;
    [SerializeField] private bool resetOnEnable = true;

    public float currentValue;

    protected override void OnEnable()
    {
        if(this.currentValue == null || this.resetOnEnable)
        {
            this.Execute();
        }
        base.OnEnable();
    }

    public override object GetValue()
    {
        return this.currentValue;
    }

    public override void Execute()
    {
        this.currentValue = (float)Random.Range(this.range.x, this.range.y);
    }
}
