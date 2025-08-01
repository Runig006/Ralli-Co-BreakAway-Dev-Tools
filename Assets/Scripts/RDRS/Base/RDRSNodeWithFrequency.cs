using System.Collections;
using UnityEngine;

public abstract class RDRSNodeWithFrequency : RDRSNode
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
                this.Execute();
                yield return null;
            }
        }
        else
        {
            WaitForSeconds wait = new WaitForSeconds(this.frequency);
            while (true)
            {
                this.Execute();
                yield return wait;
            }
        }
    }
}
