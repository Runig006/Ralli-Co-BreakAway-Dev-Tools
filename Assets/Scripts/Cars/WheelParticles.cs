using System;
using UnityEngine;

public class WheelParticles : MonoBehaviour
{
	private SuspensionPhysic physic;
	[SerializeField] private float driftMinSideForce = 0.7f;
	
	private GameObject driftParent;
	private GameObject dustParent;
	
	private GameObject driftGameObject;
	private GameObject dustGameObject;
	
	private ParticleSystem dustParticleSystem;
	private ParticleSystem driftParticleSystem;
	
	private TerrainInfo currentTerrain;
	
	//Some of the "DriftParents" are in the back because the wheels that stop "turning" are the fronts one, but is more impresive is they are in the back
	void Start()
	{
		this.physic = transform.parent.parent.Find("Suspension").Find(this.name).GetComponent<SuspensionPhysic>();
		this.driftParent = transform.Find("DriftParent").gameObject;
		this.dustParent = transform.Find("DustParent").gameObject;
	}

	void Update()
	{
		this.UpdateCurrentTerrain();
		this.UpdateDrift();
		this.UpdateDust();
		
		if(this.driftParticleSystem)
		{
			this.CheckForDrift();
		}
		
		if(this.dustParticleSystem)
		{
			this.CheckForDust();
		}
	}
	
	void UpdateCurrentTerrain()
	{
		var newTerrain = this.physic.GetCurrentTerrain();
		if (newTerrain != this.currentTerrain)
		{
			this.DestroyDustParticleSystem();
			this.DestroyDriftParticleSystem();
		}
		this.currentTerrain = newTerrain;
	}

	void UpdateDrift()
	{
		if (this.currentTerrain == null || this.currentTerrain.GetDriftGameObject() == null)
		{
			this.DestroyDriftParticleSystem();
		}
		else if (this.driftGameObject == null)
		{
			this.driftGameObject = Instantiate(this.currentTerrain.GetDriftGameObject());
			this.driftGameObject.transform.parent = this.driftParent.transform;
			this.driftGameObject.transform.localPosition = Vector3.zero;
			this.driftGameObject.transform.localEulerAngles = Vector3.zero;
			
			this.driftParticleSystem = this.driftGameObject.GetComponent<ParticleSystem>();
		}
	}
	
	void UpdateDust()
	{
		if (this.currentTerrain == null || this.currentTerrain.GetDustGameObject() == null)
		{
			this.DestroyDustParticleSystem();
		}
		
		else if (this.dustGameObject == null)
		{
			this.dustGameObject = Instantiate(this.currentTerrain.GetDustGameObject() );
			this.dustGameObject.transform.parent = this.dustParent.transform;
			this.dustGameObject.transform.localPosition = Vector3.zero;
			this.dustGameObject.transform.localEulerAngles = Vector3.zero;
			
			this.dustParticleSystem = this.dustGameObject.GetComponent<ParticleSystem>();
		}
	}
	
	void CheckForDust()
	{
		if(this.physic.GetGrounded())
		{
			ChangeParticleSystem(this.dustParticleSystem, true);
		}else
		{
			ChangeParticleSystem(this.dustParticleSystem, false);
		}
	}
	
	void CheckForDrift()
	{
		if(this.physic.GetSideForce() > this.driftMinSideForce && this.physic.GetGrounded())
		{
			ChangeParticleSystem(this.driftParticleSystem, true);
		}else
		{
			ChangeParticleSystem(this.driftParticleSystem, false);
		}
	}
	
	void ChangeParticleSystem(ParticleSystem s, bool status)
	{
		if (status && s.isEmitting == false)
		{
			s.Play();
		}
		else if (status == false && s.isEmitting == true)
		{
			s.Stop();
		}
	}
	
	private void DestroyDriftParticleSystem()
	{
		if (this.driftGameObject != null)
		{
			Destroy(this.driftGameObject);
			this.driftGameObject = null;
			this.driftParticleSystem = null;
		}
	}
	
	private void DestroyDustParticleSystem()
	{
		if (this.dustGameObject != null)
		{
			Destroy(this.dustGameObject);
			this.dustGameObject = null;
		}
	}
}