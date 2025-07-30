using UnityEngine;

public class DebugExecutor : RDRSExecutorWithFrequency
{
    [SerializeField] private RDRSReaderBase valueReader;
    [SerializeField] private string debugOutput;

    public override object GetExecuteValue()
    {
        return this.valueReader?.GetValue();
    }

    public override void Execute()
    {
        object value = this.GetExecuteValue();
        string message;

        if (value == null)
        {
            message = "[DebugExecutor] Value is null.";
        }
        else
        {
            message = $"[DebugExecutor] Type: {value.GetType().Name}, Value: {value}";
        }
        this.debugOutput = message;
    }
}
