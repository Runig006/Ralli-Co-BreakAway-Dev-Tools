using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class DashboardDigitalTextColorElement : MonoBehaviour
{
	[SerializeField] private DashboardPrintableValuesEnum printing;
	[SerializeField] private bool normalize;
	[SerializeField] private string numberFormat;
	[SerializeField] private bool invert;
	[SerializeField] private Color defaultColor = Color.white;
	
	[Tooltip("Define thresholds and colors to change text color based on value.")]
	[SerializeField] private List<ThresholdColor> thresholdColors = new List<ThresholdColor>();

	private CarParameters carParameters;
	private ScoringDetector scoringDetector;
	private TMP_Text text;
	private float currentBlinkSpeed = 0;

	void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
		this.scoringDetector = this.carParameters.gameObject.GetComponentInChildren<ScoringDetector>();
		this.text = GetComponent<TMP_Text>();
	}

	private void Update()
	{
		float value = 0;
		
		if(this.scoringDetector == null)
		{
			return;
		}
		
		switch (this.printing)
		{
			case DashboardPrintableValuesEnum.gear:
				if(this.carParameters.GetIsInReverse())
				{
					value = -1;
					break;
				}
				if(this.carParameters.GetCurrentGear() == null)
				{
					value = 0;
					break;
				}
				value = this.carParameters.GetCurrentGear() ?? 0 + 1;
				break;
			case DashboardPrintableValuesEnum.rpm:
				value =  this.normalize ? this.carParameters.GetRPMNormalize() : this.carParameters.GetFakeRPM();
				break;
			case DashboardPrintableValuesEnum.speed:
				value =  this.normalize ? this.carParameters.GetVelocityNormalize() : MathF.Abs(this.carParameters.GetForwardVelocity() * 3.6f);
				break;
			case DashboardPrintableValuesEnum.nitro:
				value = this.carParameters.GetBoostTemperature();
				break;
			case DashboardPrintableValuesEnum.currentTimer:
				value =  this.scoringDetector.GetTimeInLevel();
				break;
			case DashboardPrintableValuesEnum.remainTimer:
				value =  this.scoringDetector.GetRemainingTime();
				break;
			case DashboardPrintableValuesEnum.viewerTimer:
				value =  this.scoringDetector.GetRemainTimeForViewers();
				break;
			case DashboardPrintableValuesEnum.currentViewers:
				value =  this.scoringDetector.GetCurrentViewers();
				break;
			case DashboardPrintableValuesEnum.currentRetainer:
				value =  this.scoringDetector.GetCurrentRetainer();
				break;
			default:
				value =  0;
				break;
		}
		
		foreach (var thresholdColor in thresholdColors)
		{
			if (value >= thresholdColor.threshold)
			{
				this.text.color = thresholdColor.color;
			}
			else
			{
				break;
			}
		}
		
		bool colorSet = false;
		foreach (var thresholdColor in thresholdColors)
		{
			if ((invert && value <= thresholdColor.threshold) || (!invert && value >= thresholdColor.threshold))
			{
				this.text.color = thresholdColor.color;
				colorSet = true;
				this.currentBlinkSpeed = thresholdColor.blinkSpeed;
				break;
			}
		}
		
		if (!colorSet)
		{
			this.text.color = defaultColor;
			this.currentBlinkSpeed = 0;
		}
		this.ApplyBlinkingEffect();
	}
	
	private void ApplyBlinkingEffect()
	{
		if (currentBlinkSpeed > 0)
		{
			float alpha = Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI / currentBlinkSpeed));
			Color colorWithAlpha = this.text.color;
			colorWithAlpha.a = alpha;
			this.text.color = colorWithAlpha;
		}
		else
		{
			Color colorWithAlpha = this.text.color;
			colorWithAlpha.a = 1;
			this.text.color = colorWithAlpha;
		}
	}
}