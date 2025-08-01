using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
	[SerializeField] private float powerMultipler = 1.0f;
	[SerializeField] private float gripMultiplier = 1.0f;
	[SerializeField] private float resistanceMultipler = 0.0f;
	[SerializeField] private GameObject driftGameObject; 	
	[SerializeField] private GameObject dustGameObject; 
	
	[SerializeField] [Range(0.0f, 1.0f)] private float roughness = 0.0f;

	// Getters
	public float GetPowerMultipler()
	{
		return this.powerMultipler;
	}
	
	public float GetResistanceMultipler()
	{
		return this.resistanceMultipler;
	} 
	
	public float GetGripMultipler()
	{
		return this.gripMultiplier;
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
