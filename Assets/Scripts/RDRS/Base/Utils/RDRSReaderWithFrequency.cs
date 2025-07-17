using System.Collections;
using UnityEngine;

//Its half reader, half executer. You cant make something a frequency...without doing "something", or you add, or you read, but you DO something
public abstract class RDRSReaderWithFrequency : RDRSReaderBase, IRDRSExecutor
{
    [SerializeField] protected float frequency = 0.0f;
    private Coroutine updateRoutine;

    protected virtual void OnEnable()
    {
        if (this.frequency >= 0f)
        {
            this.updateRoutine = StartCoroutine(PeriodicUpdate());
        }
    }

    protected virtual void OnDisable()
    {
        if (this.updateRoutine != null)
        {
            StopCoroutine(this.updateRoutine);
            this.updateRoutine = null;
        }
    }

    protected virtual IEnumerator PeriodicUpdate()
    {
        if (this.frequency == 0f)
        {
            while (true)
            {
                this.Execute(this.GetExecuteValue());
                yield return null;
            }
        }
        else
        {
            WaitForSeconds wait = new WaitForSeconds(this.frequency);
            while (true)
            {
                this.Execute(this.GetExecuteValue());
                yield return wait;
            }
        }
    }

    public abstract object GetExecuteValue();
    public abstract void Execute(object value);
}
