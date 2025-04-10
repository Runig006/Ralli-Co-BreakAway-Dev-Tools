using System;
using System.Globalization;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class DashboardDigitalTextElement : MonoBehaviour
{
	
	[SerializeField] private DashboardPrintableValuesEnum printing;
	[Tooltip("Only for RPM, Speed and Nitro")]
	[SerializeField] private bool normalice;
	[Tooltip("If you are using a timer value, specify the format for TimeSpan (e.g., \"hh:\\mm:\\ss\"). You can use a special value called arcade, (e.g, \"arcade:{2, 0}\") that will print the time as 99 to 1, the first parameter ammount of numbers, second parameter if you want 0 at the lefts or not. For other values, use ToString format")]
	[SerializeField] private string numberFormat;
	[SerializeField] private string prefix;
	[SerializeField] private string suffix;
	
	private CarParameters carParameters;
	private ScoringDetector scoringDetector;
	private TMP_Text text;
	
	void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
		this.scoringDetector = this.carParameters.gameObject.GetComponentInChildren<ScoringDetector>();
		this.text = GetComponent<TMP_Text>();
	}

	void Update()
	{
		string value = "";
		switch (this.printing)
		{
			case DashboardPrintableValuesEnum.gear:
				value = this.carParameters.GetIsInReverse() ? "R" : (this.carParameters.GetCurrentGear() + 1).ToString();
				break;
			case DashboardPrintableValuesEnum.rpm:
				value = this.normalice ? this.carParameters.GetRPMNormalice().ToString(this.numberFormat, CultureInfo.InvariantCulture) : this.carParameters.GetFakeRPM().ToString(this.numberFormat, CultureInfo.InvariantCulture);
				break;
			case DashboardPrintableValuesEnum.speed:
				value = this.normalice ? this.carParameters.GetVelocityNormalice().ToString(this.numberFormat, CultureInfo.InvariantCulture) : MathF.Abs(this.carParameters.GetForwardVelocity() * 3.6f).ToString(this.numberFormat, CultureInfo.InvariantCulture);
				break;
			case DashboardPrintableValuesEnum.nitro:
				value = this.carParameters.GetBoostTemperature().ToString(this.numberFormat, CultureInfo.InvariantCulture);
				break;
			case DashboardPrintableValuesEnum.currentTimer:
				value = this.FormatTime(this.scoringDetector.GetTimeInLevel());
				break;
			case DashboardPrintableValuesEnum.remainTimer:
				value = this.FormatTime(this.scoringDetector.GetRemainingTime());
				break;
			case DashboardPrintableValuesEnum.viewerTimer:
				value = this.FormatTime(this.scoringDetector.GetRemainTimeForViewers());
				break;
			case DashboardPrintableValuesEnum.currentViewers:
				value = this.scoringDetector.GetCurrentViewers().ToString(this.numberFormat, CultureInfo.InvariantCulture);
				break;
			case DashboardPrintableValuesEnum.currentRetainer:
				value = this.scoringDetector.GetCurrentRetainer().ToString(this.numberFormat, CultureInfo.InvariantCulture);
				break;
		}
		
		this.text.text = this.prefix + value + this.suffix;
	}

	private string FormatTime(float totalSeconds)
	{
		if (this.numberFormat.StartsWith("arcade:"))
		{
			return this.FormatArcade(totalSeconds);
		}
		TimeSpan timeSpan = TimeSpan.FromSeconds(totalSeconds);
		return !string.IsNullOrEmpty(this.numberFormat) ? timeSpan.ToString(this.numberFormat, CultureInfo.InvariantCulture) : timeSpan.ToString(@"mm\:ss\:ff");
	}
	
	private string FormatArcade(float totalSeconds)
	{
		string parameters = this.numberFormat.Substring(7).Trim('{', '}');
		string[] parts = parameters.Split(',');
		int maxDigits = 2;
		bool leadingZeros = false;
		
		if(parts.Length > 0)
		{
			maxDigits = int.Parse(parts[0]);
		}
		
		if(parts.Length > 1)
		{
			leadingZeros = int.Parse(parts[1]) > 0;
		}
		
		int totalSecondsInt = Mathf.FloorToInt(totalSeconds);
		string formatString = leadingZeros ? totalSecondsInt.ToString().PadLeft(maxDigits, '0') : totalSecondsInt.ToString().PadLeft(maxDigits);
		if (formatString.Length > maxDigits){
			formatString = formatString.Substring(0, maxDigits);
		}

		return formatString;
	}
}
