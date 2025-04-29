using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TriggerController : MonoBehaviour
{
	[SerializeField] private float actionsPerFrame = 1.0f;
	[SerializeField] private MonoBehaviour[] toEnable;
	[SerializeField] private MonoBehaviour[] toDisable;
	[SerializeField] private GameObject[] gameObjectsToEnable;
	[SerializeField] private GameObject[] gameObjectsToDisable;
	[SerializeField] private bool triggerOnlyOnce = true;

	private bool hasTriggered = false;

	private Dictionary<MonoBehaviour, bool> originalStates = new Dictionary<MonoBehaviour, bool>();
	private Dictionary<GameObject, bool> originalObjectsStates = new Dictionary<GameObject, bool>();

	void Start()
	{
		foreach (MonoBehaviour comp in this.toEnable)
		{
			if (comp != null && this.originalStates.ContainsKey(comp) == false)
			{
				this.originalStates[comp] = comp.enabled;
			}
		}

		foreach (MonoBehaviour comp in this.toDisable)
		{
			if (comp != null && this.originalStates.ContainsKey(comp) == false)
			{
				this.originalStates[comp] = comp.enabled;
			}
		}

		foreach (GameObject go in this.gameObjectsToEnable)
		{
			if (go != null && this.originalObjectsStates.ContainsKey(go) == false)
			{
				this.originalObjectsStates[go] = go.activeSelf;
			}
		}

		foreach (GameObject go in this.gameObjectsToDisable)
		{
			if (go != null && this.originalObjectsStates.ContainsKey(go) == false)
			{
				this.originalObjectsStates[go] = go.activeSelf;
			}
		}
	}

	private void OnTriggerEnter(Collider collider)
	{
		if (this.triggerOnlyOnce && this.hasTriggered)
		{
			return;
		}

		if (collider.CompareTag("PlayerRealBody") == false)
		{
			return;
		}

		this.hasTriggered = true;
		StartCoroutine(ApplyChangesGradually());
	}

	private IEnumerator ApplyChangesGradually()
	{
		int actionsDone = 0;

		foreach (MonoBehaviour comp in this.toEnable)
		{
			if (comp != null)
			{
				comp.enabled = true;
				actionsDone++;
				if (actionsDone >= actionsPerFrame)
				{
					actionsDone = 0;
					yield return null;
				}
			}
		}

		foreach (MonoBehaviour comp in this.toDisable)
		{
			if (comp != null)
			{
				comp.enabled = false;
				actionsDone++;
				if (actionsDone >= actionsPerFrame)
				{
					actionsDone = 0;
					yield return null;
				}
			}
		}

		foreach (GameObject go in this.gameObjectsToEnable)
		{
			if (go != null)
			{
				go.SetActive(true);
				actionsDone++;
				if (actionsDone >= actionsPerFrame)
				{
					actionsDone = 0;
					yield return null;
				}
			}
		}

		foreach (GameObject go in this.gameObjectsToDisable)
		{
			if (go != null)
			{
				go.SetActive(false);
				actionsDone++;
				if (actionsDone >= actionsPerFrame)
				{
					actionsDone = 0;
					yield return null;
				}
			}
		}
	}
}
