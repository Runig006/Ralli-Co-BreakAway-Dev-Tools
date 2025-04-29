using UnityEngine;

public class BoostBurnParticle : MonoBehaviour
{
	private CarParameters carParameters;
	private ParticleSystem[] turboParticles;
	private bool enabledParticles;

	void Start()
	{
		this.carParameters = this.GetComponentInParent<CarParameters>();
		this.turboParticles = this.GetComponentsInChildren<ParticleSystem>();
	}

	void Update()
	{
		if(this.carParameters.GetBoostBurn() && this.enabledParticles == false)
		{
			foreach(ParticleSystem ps in this.turboParticles)
			{
				var psMain = ps.main;
				psMain.loop = true;
				ps.Play();
			}
			this.enabledParticles = true;
		}
		else if (this.carParameters.GetBoostBurn() == false && this.enabledParticles == true)
		{
			foreach (ParticleSystem ps in this.turboParticles)
			{
				var psMain = ps.main;
				psMain.loop = false;
			}
			this.enabledParticles = false;
		}
	}
}
