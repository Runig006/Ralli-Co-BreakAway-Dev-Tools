using UnityEngine;

public class CarLightManager : MonoBehaviour
{
	CarParameters carParameters;
	[SerializeField] Light[] frontsLight;
	[SerializeField] Light[] brakesLights;
	[SerializeField] MeshRenderer frontLightRender;
	[SerializeField] MeshRenderer backLightRender;
	[SerializeField] int frontLightMaterialIndex;
	[SerializeField] int backLightMaterialIndex;
	
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
			this.frontLightRender.materials[this.frontLightMaterialIndex].EnableKeyword("_EMISSION");
			this.backLightRender.materials[this.backLightMaterialIndex].EnableKeyword("_EMISSION");
		}
		else
		{
			this.frontLightRender.materials[this.frontLightMaterialIndex].DisableKeyword("_EMISSION");
			this.backLightRender.materials[this.backLightMaterialIndex].DisableKeyword("_EMISSION");
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
