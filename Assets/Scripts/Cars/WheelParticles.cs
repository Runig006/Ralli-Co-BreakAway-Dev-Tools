using System.Collections;
using UnityEngine;

public class WheelParticles : MonoBehaviour
{
	private SuspensionPhysic physic;
	private CarParameters carParameters;
	
	private GameObject driftParent;
	private GameObject dustParent;
	
	private ParticleSystem dustParticleSystem;
	private ParticleSystem driftParticleSystem;
	
	private TerrainInfo currentTerrain;
	
	private void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
		
		this.physic = this.carParameters.transform.Find("Suspension").Find(this.name).GetComponent<SuspensionPhysic>();
		
		this.driftParent = new GameObject("DriftParent").gameObject;
		this.dustParent = new GameObject("DustParent").gameObject;
		this.driftParent.transform.parent = this.transform;
		this.dustParent.transform.parent = this.transform;
		
		this.driftParent.transform.localPosition = Vector3.zero;
		this.driftParent.transform.localRotation = Quaternion.Euler(0, -180, 0);
		this.dustParent.transform.localPosition = Vector3.zero;
		this.dustParent.transform.localRotation = Quaternion.Euler(0, -180, 0);
	}

	private void Update()
	{
		this.UpdateCurrentTerrain();
		
		this.UpdateDrift();
		this.UpdateDust();
		
		if(this.driftParticleSystem != null)
		{
			ChangeParticleSystem(this.driftParticleSystem, this.physic.GetDriftEnable());
		}
		
		if(this.dustParticleSystem != null)
		{
			ChangeParticleSystem(this.dustParticleSystem, this.physic.GetDustEnable());
		}
	}
	
	private void UpdateCurrentTerrain()
	{
		var newTerrain = this.physic.GetCurrentTerrain();
		if (newTerrain != this.currentTerrain)
		{
			this.SetToDestroyDrift();
			this.SetToDestroyDust();
		}
		this.currentTerrain = newTerrain;
	}

	private void UpdateDrift()
	{
		if (this.currentTerrain == null || this.currentTerrain.GetDriftGameObject() == null)
		{
			return;
		}
		
		if (this.driftParticleSystem == null)
		{
			this.driftParticleSystem = Instantiate(this.currentTerrain.GetDriftGameObject()).GetComponent<ParticleSystem>();
			this.driftParticleSystem.gameObject.transform.parent = this.driftParent.transform;
			this.driftParticleSystem.gameObject.transform.localPosition = Vector3.zero;
			this.driftParticleSystem.gameObject.transform.localEulerAngles = Vector3.zero;
		}
	}
	
	private void UpdateDust()
	{
		if (this.currentTerrain == null || this.currentTerrain.GetDustGameObject() == null)
		{
			return;
		}
		
		if (this.dustParticleSystem == null)
		{
			this.dustParticleSystem = Instantiate(this.currentTerrain.GetDustGameObject()).GetComponent<ParticleSystem>();
			this.dustParticleSystem.gameObject.transform.parent = this.dustParent.transform;
			this.dustParticleSystem.gameObject.transform.localPosition = Vector3.zero;
			this.dustParticleSystem.gameObject.transform.localEulerAngles = Vector3.zero;
		}
	}
	
	private void ChangeParticleSystem(ParticleSystem s, bool status)
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
	
	private void SetToDestroyDrift()
	{
		if (this.driftParticleSystem == null)
		{
			return;
		}

		ParticleSystem toDestroy = this.driftParticleSystem;
		this.driftParticleSystem = null;

		StartCoroutine(WaitForParticlesToEnd(toDestroy, () =>
		{
			Destroy(toDestroy.gameObject);
		}));
	}

	private void SetToDestroyDust()
	{
		if (this.dustParticleSystem == null)
		{
			return;
		}

		ParticleSystem toDestroy = this.dustParticleSystem;
		this.dustParticleSystem = null;
		
		StartCoroutine(WaitForParticlesToEnd(toDestroy, () =>
		{
			Destroy(toDestroy.gameObject);
		}));
	}
	
	private IEnumerator WaitForParticlesToEnd(ParticleSystem ps, System.Action onComplete)
	{
		if (ps == null) yield break;

		ps.Stop();
		var em = ps.emission;
		em.rateOverDistance = 0.0f;
		em.rateOverDistance = 0.0f;
		
		yield return new WaitForSeconds(0.5f); 

		while (ps.IsAlive(true))
		{
			yield return new WaitForSeconds(0.5f);
		}

		onComplete?.Invoke();
	}
}