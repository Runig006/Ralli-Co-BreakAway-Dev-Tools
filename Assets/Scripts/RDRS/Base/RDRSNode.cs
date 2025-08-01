using System;
using UnityEngine;

public abstract class RDRSNode : MonoBehaviour
{
    public string Tag;

    public virtual object GetValue()
	{
		throw new NotImplementedException($"{GetType().Name} GetValue() is not implemented ");
	}

	public virtual void Execute()
	{
		throw new NotImplementedException($"{GetType().Name} Execute() is not implemented");
	}
}
