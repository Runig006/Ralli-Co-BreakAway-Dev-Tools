using UnityEngine;

public class CarLightManager : MonoBehaviour
{
	CarParameters carParameters;
	[SerializeField] Light[] frontsLight;
	[SerializeField] Light[] brakesLights;
	[SerializeField] Material frontLightMaterial;
	[SerializeField] Material backLightMaterial;
	
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
	}
	
	public void SetLight(bool status)
	{
		foreach(Light light in frontsLight)
		{
			light.enabled = status;
		}
		if(status)
		{
			frontLightMaterial.EnableKeyword("_EMISSION");
			backLightMaterial.EnableKeyword("_EMISSION");
		}
		else
		{
			frontLightMaterial.DisableKeyword("_EMISSION");
			backLightMaterial.DisableKeyword("_EMISSION");
		}
	}

	// Update is called once per frame
	void Update()
	{
		if(this.carParameters.GetBrake() > 0)
		{
			foreach(Light light in brakesLights)
			{
				light.enabled = true;
			}
		}
		else
		{
			foreach(Light light in brakesLights)
			{
				light.enabled = false;
			}
		}
	}
}
