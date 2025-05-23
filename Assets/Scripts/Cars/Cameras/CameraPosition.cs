using UnityEngine;

public class CameraPosition : MonoBehaviour
{
	[SerializeField] private bool interior = true;
	[SerializeField] private float engineVolume = 1.0f;
	[SerializeField] private int baseFov = 45;
	
	[SerializeField] bool hideTopLeftUi;
	[SerializeField] bool hideTopCenterUi;
	[SerializeField] bool hideTopRightUi;
	[SerializeField] bool hideBottomLeftUi;
	[SerializeField] bool hideBottomRightUi;
	[SerializeField] bool hideRemainerTimeUi;
	
	[Header("Options for Cinemachine Follow")]
	[Header("This is ONLY for the rotation of the camera. Less speed, more rotation")]
	[SerializeField] private Vector3 maxRotationDamping = new Vector3(1.0f,5.0f,0.0f);
	[SerializeField] private Vector3 minRotationDamping = new Vector3(1.0f,1.0f,0.0f);
	[SerializeField] private float maxSpeedForDamping = 70.0f;
	
	[Header("Used when the car is in a slope")]
	[SerializeField] private float minHeightOffset = 1.7f;
	[SerializeField] private float maxHeightOffset = 4.5f;

	[Header("Limit the camera effects")]
	[Range(0, 1)][SerializeField] private float fovMultiplier = 1.0f;
	[Range(0, 1)][SerializeField] private float distorsionMultiplier = 1.0f;
	[Range(0, 1)][SerializeField] private float chromaticMultiplier = 1.0f;
	[Range(0, 1)][SerializeField] private float shakeMultiplier = 1.0f;
	
	
	public bool GetInterior()
	{
		return this.interior;
	}
	
	public float GetEngineVolume()
	{
		return this.engineVolume;
	}
	
	public int GetBaseFov()
	{
		return this.baseFov;
	}
	
	public void SetBaseFov(int baseFov)
	{
		this.baseFov = baseFov;
	}
	
	public bool GetHideTopLeftUi()
	{
		return this.hideTopLeftUi;
	}
	
	public bool GetHideTopRightUi()
	{
		return this.hideTopRightUi;
	}
	
		
	public bool GetHideTopCenterUi()
	{
		return this.hideTopCenterUi;
	}
	
	public bool GetHideBottomLeftUi()
	{
		return this.hideBottomLeftUi;
	}
	
	public bool GetHideBottomRightUi()
	{
		return this.hideBottomRightUi;
	}
	
	public bool GetHideRemainertUi()
	{
		return this.hideRemainerTimeUi;
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
	
	public float GetMinHeightOffset()
	{
		return this.minHeightOffset;
	}
	
	public float GetMaxHeightOffset()
	{
		return this.maxHeightOffset;
	}

	public float GetFovMultiplier()
	{
		return this.fovMultiplier;
	}
	
	public float GetDistorsionMultiplier()
	{
		return this.distorsionMultiplier;
	}
	
	public float GetChromaticMultiplier()
	{
		return this.chromaticMultiplier;
	}
	
	public float GetShakeMultiplier()
	{
		return this.shakeMultiplier;
	}
}
