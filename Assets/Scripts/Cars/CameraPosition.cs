using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	[SerializeField] private bool interior = true;
	[SerializeField] private float masterVolume = 1.0f;
	
	public bool GetInterior()
	{
		return this.interior;
	}
	
	public float GetMasterVolume()
	{
		return this.masterVolume;
	}
}
