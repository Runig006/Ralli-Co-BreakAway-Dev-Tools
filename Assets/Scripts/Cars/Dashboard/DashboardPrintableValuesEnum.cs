using System;
using UnityEngine;

public enum DashboardPrintableValuesEnum
	{
		gear,
		rpm,
		speed,
		nitro,
		currentTimer,
		remainTimer,
		viewerTimer,
		currentViewers,
		currentRetainer,
}

[Serializable]
public struct ThresholdColor
{
	public float threshold;
	public Color color;
	public float blinkSpeed;
}