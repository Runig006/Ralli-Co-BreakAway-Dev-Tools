using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	[SerializeField] private bool interior = true;
	[SerializeField] private float masterVolume = 1.0f;
	[SerializeField] private int baseFov = 45;
	
	[Header("Options for Cinemachine Follow")]
	[SerializeField] private Vector3 maxRotationDamping = new Vector3(1.0f,5.0f,0.0f);
	[SerializeField] private Vector3 minRotationDamping = new Vector3(1.0f,1.0f,0.0f);
	[SerializeField] private float maxSpeedForDamping = 70.0f;
	
	
	public bool GetInterior()
	{
		return this.interior;
	}
	
	public float GetMasterVolume()
	{
		return this.masterVolume;
	}
	
	public int GetBaseFov()
	{
		return this.baseFov;
	}
	
	public void SetBaseFov(int baseFov)
	{
		this.baseFov = baseFov;
	}
	
	public Vector3 GetMaxRotationDamping()
	{
		return this.maxRotationDamping;
	}
	
	public Vector3 GetMinRotationDamping()
	{
		return this.minRotationDamping;
	}

	public float GetMaxSpeedForDamping()
	{
		return this.maxSpeedForDamping;
	}
}
