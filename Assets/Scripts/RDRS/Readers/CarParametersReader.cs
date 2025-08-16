using System;
using UnityEngine;

public class CarParametersReader : RDRSNode
{
    public enum CarReaderParameterType
    {
        Speed = 0,
        NormalizeSpeed = 1,
        Gear = 2,
        RPM = 3,
        NormalizeRPM = 4,
        IsBosting = 5,
        IsBurned = 6,
        BoostTemperature = 7,
        Grounded = 9,
        ThrottleInput = 10,
        BrakeInput = 11,
        SteerInput = 12,
        LightsStatus = 13,
        IsSlipStream = 14,
        SlipstreamTimer = 15,
        Position = 16,
        Rotation = 17
    }

    
    [SerializeField] private CarReaderParameterType parameter;
    [SerializeField] private CarParameters carParameters;
    [SerializeField] private bool autoFromParent = true;
    [SerializeField] private bool findByDistance = false;
    [SerializeField] private int playerID = -1;
    [SerializeField] private float maxDistance = 20000.0f;

    public override object GetValue()
    {
        if (this.findByDistance)
        {
            this.FindClosestByDistance();
        }
        else if (this.carParameters == null)
        {
            this.FindCarParameter();
        }

        if (this.carParameters == null)
        {
            return null;
        }

        switch (this.parameter)
        {
            case CarReaderParameterType.Speed:
                return this.carParameters.GetForwardVelocity();
            case CarReaderParameterType.NormalizeSpeed:
                return this.carParameters.GetVelocityNormalize();
            case CarReaderParameterType.Gear:
                int? gear = this.carParameters.GetCurrentGear();
                if (gear == null)
                {
                    return gear;
                }
                if (gear == -1)
                {
                    return -1.0f;
                }
                return (float)gear + 1;
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
            case CarReaderParameterType.Grounded:
                return this.carParameters.GetGrounded() ? 1f : 0f;
            case CarReaderParameterType.ThrottleInput:
                return this.carParameters.GetThrottle();
            case CarReaderParameterType.BrakeInput:
                return this.carParameters.GetBrake();
            case CarReaderParameterType.SteerInput:
                return this.carParameters.GetTurn();
            case CarReaderParameterType.LightsStatus:
                return this.carParameters.GetLightsStatus();
            case CarReaderParameterType.IsSlipStream:
                return this.carParameters.GetIsSlipstreaming();
            case CarReaderParameterType.SlipstreamTimer:
                return this.carParameters.GetSlipstreamTime();
            case CarReaderParameterType.Position:
                return this.carParameters.gameObject.transform.position;
            case CarReaderParameterType.Rotation:
                return this.carParameters.gameObject.transform.rotation;
            default: //How?
                return 0f;
        }
    }

    
    private void FindCarParameter()
    {
        if (this.autoFromParent)
        {
            this.carParameters = GetComponentInParent<CarParameters>();
            if (this.carParameters != null)
            {
                return;
            }
        }
        
       
        if (this.playerID >= 0)
        {
            #if FULL_GAME
                CarManager carManager = FindFirstObjectByType<CarManager>();
                if (carManager != null)
                {
                    GameObject carGO = carManager.GetCarByPlayer(playerID);
                    this.carParameters = carGO.GetComponent<CarParameters>();
                
                }
            #else
                this.carParameters = FindFirstObjectByType<CarParameters>()?.GetComponent<CarParameters>();
            #endif
        }
    }

    private void FindClosestByDistance()
    {
        CarParameters[] allCars = FindObjectsByType<CarParameters>(FindObjectsSortMode.None);
        if (allCars.Length == 0)
        {
            return;
        }

        CarParameters closest = null;
        float closestSqrDistance = this.maxDistance * this.maxDistance;

        foreach (CarParameters car in allCars)
        {
            float sqrDist = (car.transform.position - this.transform.position).sqrMagnitude;
            if (sqrDist < closestSqrDistance)
            {
                closestSqrDistance = sqrDist;
                closest = car;
            }
        }

        this.carParameters = closest;
    }

}
