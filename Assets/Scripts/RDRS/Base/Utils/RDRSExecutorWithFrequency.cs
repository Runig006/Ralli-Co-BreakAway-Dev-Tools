using System.Collections;
using UnityEngine;

public abstract class RDRSExecutorWithFrequency : RDRSExecutorBase, IRDRSExecutor
{
    [SerializeField] protected float frequency = 0.0f;
    private Coroutine updateRoutine;

    public virtual void OnEnable()
    {
        if (this.frequency >= 0f)
        {
            this.updateRoutine = StartCoroutine(PeriodicUpdate());
        }
    }

    public virtual void OnDisable()
    {
        if (this.updateRoutine != null)
        {
            StopCoroutine(this.updateRoutine);
            this.updateRoutine = null;
        }
    }

    public virtual IEnumerator PeriodicUpdate()
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
}
