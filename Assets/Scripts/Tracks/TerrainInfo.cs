using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
	[SerializeField] private float powerMultipler = 1.0f;
	[SerializeField] private float gripMultiplier = 1.0f;
	[SerializeField] private float gripBackMultiplier = 1.0f;
	[SerializeField] private bool enableDriftSound = true; 
	[SerializeField] private GameObject driftGameObject; 	
	[SerializeField] private GameObject dustGameObject; 	
	
	[SerializeField] [Range(0.0f, 1.0f)] private float roughness = 0.0f;

	// Getters
	public float GetPowerMultipler()
	{
		return this.powerMultipler;
	}
	public float GetGripMultipler()
	{
		return this.gripMultiplier;
	}
	public float GetBackGripMultipler()
	{
		return this.gripBackMultiplier;
	}
	
	public bool GetEnableDriftSound()
	{
		return this.enableDriftSound;
	}
	
	public GameObject GetDriftGameObject()
	{
		return this.driftGameObject;
	}
	
	public GameObject GetDustGameObject()
	{
		return this.dustGameObject;
	}
	
	//For FFB
	public float GetRoughness()
	{
		return this.roughness;
	}
}
