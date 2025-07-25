using UnityEngine;

public class TerrainInfo : MonoBehaviour
{
	[SerializeField] private float powerMultipler = 1.0f;
	[SerializeField] private float gripMultiplier = 1.0f;
	[SerializeField] private float resistanceMultipler = 0.0f;
	[SerializeField] private GameObject driftParticlesGameObject; 	
	[SerializeField] private GameObject dustParticlesGameObject; 
	[SerializeField] private GameObject driftSoundGameObject; 	
	[SerializeField] private GameObject dustSoundGameObject; 
	
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
		return this.driftParticlesGameObject;
	}
	
	public GameObject GetDustGameObject()
	{
		return this.dustParticlesGameObject;
	}

	public GameObject GetDriftSound()
	{
		return this.driftSoundGameObject;
	}
			
	public GameObject GetDustSound()
	{
		return this.dustSoundGameObject;
	}

	
	//For FFB
	public float GetRoughness()
	{
		return this.roughness;
	}
}
