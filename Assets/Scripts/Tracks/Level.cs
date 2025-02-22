using System;
using UnityEngine;
using UnityEngine.Splines;

public class Level : MonoBehaviour
{
	[Header("Info")]
	[SerializeField] private string Name;
	[SerializeField] private Sprite Icon;
	[SerializeField] private Sprite BackgroundPhoto;
	[SerializeField] private float Dificulty = 2;
	
	[Header("Time and Traffic")]
	[SerializeField] private float TimePar = 75.0f;
	[SerializeField] private float TrafficStart = 0.01f;
	[SerializeField] private float TrafficDensitySteps = 0.01f;
	
	[Header("Road parts (Can be set or they will try to fill it automatic)")]
	[SerializeField] private SplineContainer[] roadSplineContainers;
	[SerializeField] private Transform startPoint;
	[SerializeField] private Transform endPoint;
	[SerializeField] private SkyBoxInfo SkyBoxInfo;
		
	[Header("Heavy Loaders (Objects that will be activated progressively)")]
	[SerializeField] private GameObject[] heavyLoaders;
	
	public void Awake()
	{
		if(this.roadSplineContainers == null || this.roadSplineContainers.Length == 0){
			this.roadSplineContainers = this.GetComponentsInChildren<SplineContainer>();
		}
		
		if(this.startPoint == null)
		{
			this.startPoint = transform.Find("HookPoints").Find("StartPoint");
		}
		
		if(this.endPoint == null)
		{
			this.endPoint = transform.Find("HookPoints").Find("EndPoint");
		}
		if(this.SkyBoxInfo == null)
		{
			this.SkyBoxInfo = this.GetComponent<SkyBoxInfo>();
		}
	}
	
	public string GetName()
	{
		return this.Name;
	}

	public Sprite GetIcon()
	{
		return this.Icon;
	}

	public Sprite GetBackgroundPhoto()
	{
		return this.BackgroundPhoto;
	}

	public float GetDificulty()
	{
		return this.Dificulty;
	}

	public float GetTimePar()
	{
		return this.TimePar;
	}

	public float GetTrafficStart()
	{
		return this.TrafficStart;
	}

	public float GetTrafficDensitySteps()
	{
		return this.TrafficDensitySteps;
	}
	
	public SplineContainer[] GetRoadContainers()
	{
		return this.roadSplineContainers;
	}
	
	public Transform GetStartPoint()
	{
		return this.startPoint;
	}
	
	public Transform GetEndPoint()
	{
		return this.endPoint;
	}
	
	public SkyBoxInfo GetSkyBoxInfo()
	{
		return this.SkyBoxInfo;
	}
	
	public GameObject[] GetHeavyLoaders()
	{
		return this.heavyLoaders;
	}
}