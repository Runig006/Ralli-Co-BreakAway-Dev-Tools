using UnityEngine;

public class DebugExecutor : RDRSNodeWithFrequency
{
    [SerializeField] private RDRSNode valueReader;
    [SerializeField] private string debugOutput;


    public override void Execute()
    {
        object value = this.valueReader?.GetValue();
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
