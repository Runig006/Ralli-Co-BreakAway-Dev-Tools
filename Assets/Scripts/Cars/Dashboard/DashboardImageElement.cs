using System;
using UnityEngine;
using UnityEngine.UI;

public class DashboardImageElement : MonoBehaviour
{
    [SerializeField] private DashboardPrintableValuesEnum printing;
    private CarParameters carParameters;
    private ScoringDetector scoringDetector;
    private Image image;
    
    void Start()
    {
        this.carParameters = GetComponentInParent<CarParameters>();
        this.scoringDetector = this.carParameters.gameObject.GetComponentInChildren<ScoringDetector>();
        this.image = GetComponent<Image>();
    }

    void Update()
    {
        float value = 1;
        
        if(this.scoringDetector == null)
		{
			return;
		}
        
        switch (this.printing)
        {
            case DashboardPrintableValuesEnum.gear:
                Debug.LogError("Gear is not valid for this input");
                break;
            case DashboardPrintableValuesEnum.rpm:
                value = this.carParameters.GetRPMNormalize();
                break;
            case DashboardPrintableValuesEnum.speed:
                value = this.carParameters.GetVelocityNormalize();
                break;
            case DashboardPrintableValuesEnum.nitro:
                value = this.carParameters.GetBoostTemperature();
                break;
            case DashboardPrintableValuesEnum.currentTimer:
                Debug.LogError("Current Timer is not valid for this input");
                break;
            case DashboardPrintableValuesEnum.remainTimer:
               	Debug.LogError("Remain Timer is not valid for this input");
                break;
            case DashboardPrintableValuesEnum.viewerTimer:
                Debug.LogError("Viewer Timer is not valid for this input");
                break;
            case DashboardPrintableValuesEnum.currentViewers:
                Debug.LogError("Current Viwers is not valid for this input");
                break;
            case DashboardPrintableValuesEnum.currentRetainer:
                Debug.LogError("Current Retainer is not valid for this input");
                break;
        }
        
        this.image.fillAmount = value;
    }
}
