using UnityEngine;

public abstract class RDRSExecutorBase : MonoBehaviour, IRDRSExecutor
{
    public string Tag;
    public abstract void Execute();
}
