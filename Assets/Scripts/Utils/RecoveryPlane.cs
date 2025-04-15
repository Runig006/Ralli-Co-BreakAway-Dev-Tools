using UnityEngine;

public class RecoveryPlane : MonoBehaviour
{
	private void OnTriggerEnter(Collider collider)
	{
		if (collider.CompareTag("PlayerRealBody") == true)
		{
			Debug.Log("Car Go BRRRRRRRRR");
		}
	}
}
