using UnityEngine;

public class CarParametersReader : RDRSReaderBase
{

    public enum CarReaderParameterType
    {
        //Values
        Speed,
        RawSpeed,
        NormalizeSpeed,
        Gear,
        RPM,
        NormalizeRPM,
        IsBosting,
        IsBurned,
        BoostTemperature,
        BrakeHotTime,
        Grounded,
        
        //Inputs
        ThrottleInput,
        BrakeInput,
        SteerInput,
        
        //Lights
        LightsStatus,
    }

    [SerializeField] private bool autoFromParent = true;
    [SerializeField] private int playerID = -1;
    [SerializeField] private CarReaderParameterType parameter;

    private CarParameters carParameters;

    void Awake()
    {
       this.FindCarParameter();
    }

    public override object GetValue()
    {
        if (this.carParameters == null)
        {
            return 0f;
        }

        switch (this.parameter)
        {
            case CarReaderParameterType.Speed:
                return this.carParameters.GetVelocityTraslated();
            case CarReaderParameterType.RawSpeed:
                return this.carParameters.GetForwardVelocity();
            case CarReaderParameterType.NormalizeSpeed:
                return this.carParameters.GetVelocityNormalize();
            case CarReaderParameterType.Gear:
                int? gear = this.carParameters.GetCurrentGear();
                return (float) (gear.HasValue ? gear.Value + 1 : -1);
            case CarReaderParameterType.RPM:
                return this.carParameters.GetFakeRPM();
            case CarReaderParameterType.NormalizeRPM:
                return this.carParameters.GetRPMNormalize();
            case CarReaderParameterType.IsBosting:
                return this.carParameters.GetIsBoosting() ? 1f : 0f;
            case CarReaderParameterType.IsBurned:
                return this.carParameters.GetBoostBurn() ? 1f : 0f;
            case CarReaderParameterType.BoostTemperature:
                return this.carParameters.GetBoostTemperature();
            case CarReaderParameterType.BrakeHotTime:
                return this.carParameters.GetBrakeStrongTimer();
            case CarReaderParameterType.Grounded:
                return this.carParameters.GetGrounded() ? 1f : 0f;
            case CarReaderParameterType.ThrottleInput:
                return this.carParameters.GetThrottle();
            case CarReaderParameterType.BrakeInput:
                return this.carParameters.GetBrake();
            case CarReaderParameterType.SteerInput:
                return this.carParameters.GetTurn();
            case CarReaderParameterType.LightsStatus:
                return this.carParameters.lightEnables;
            default: //How?
                return 0f;
        }
    }
    
    private void FindCarParameter()
    {
        if(this.autoFromParent)
        {
            this.carParameters = GetComponentInParent<CarParameters>();
        }
        else
        {
            this.carParameters = FindFirstObjectByType<CarParameters>()?.GetComponent<CarParameters>();
        }
    }
}
