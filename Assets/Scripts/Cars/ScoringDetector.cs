using System.Collections.Generic;
using UnityEngine;

public class ScoringDetector : MonoBehaviour
{
	private float levelParViewers;
	
	//Timers
	private float remainingTime = 99;
	private float timeInLevel = 0;
	
	//Current Scoring
	private int currentViewers = 1000;
	private float currentRetainer = 1.20f;
	private int totalViewers = 1000;
	
	//For deltas
	private int? lastViewers;
	private float? lastTime;
	private int? deltaViewers;
	private float? deltaTime;
	private int? bestViewers; 
	private float? bestTime;
	
	public void Start()
	{
		this.enabled = true;
	}
	
	
	public void Update()
	{
		this.timeInLevel += Time.deltaTime;
		this.remainingTime -= Time.deltaTime;
		if(this.remainingTime < 0)
		{
			this.remainingTime = 0;
		}
	}
	
	
	//Getters
	public float GetRemainingTime()
	{
		return this.remainingTime;
	}

	public float GetTimeInLevel()
	{
		return this.timeInLevel;
	}

	public int GetCurrentViewers()
	{
		return this.currentViewers;
	}

	public float GetCurrentRetainer()
	{
		return this.currentRetainer;
	}

	public int GetTotalViewers()
	{
		return this.totalViewers;
	}
	
	public float GetRemainTimeForViewers()
	{
		return Mathf.Max(this.levelParViewers - this.timeInLevel, 0);
	}
	
	public int? GetBestViewers()
	{
		return this.bestViewers;
	}
	
	public float? GetBestTime()
	{
		return this.bestTime;
	}
	
	public int? GetDeltaViewers()
	{
		return this.deltaViewers;
	}
	
	public float? GetDeltaTime()
	{
		return this.deltaTime;
	}
	
	public int? GetLastViewers()
	{
		return this.lastViewers;
	}
	
	public float? GetLastTime()
	{
		return this.lastTime;
	}
}
