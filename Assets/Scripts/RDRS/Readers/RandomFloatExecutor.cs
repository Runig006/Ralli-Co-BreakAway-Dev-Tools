using UnityEngine;

public class RandomFloatExecutor : RDRSReaderWithFrequency
{
    [SerializeField] private Vector2 range;
    [SerializeField] private bool restartWithTrack = true;
    [SerializeField] private bool restartWithEnable = true;

    private float? currentValue;

    protected override void OnEnable()
    {
        if(this.currentValue == null || this.restartWithEnable)
        {
            this.Execute();
        }
        base.OnEnable();
    }

    public override object GetValue()
    {
        return this.currentValue;
    }

    public override object GetExecuteValue()
    {
        return Random.Range(this.range.x, this.range.y);
    }

    public override void Execute()
    {
        this.currentValue = (float)this.GetExecuteValue();
    }
}
