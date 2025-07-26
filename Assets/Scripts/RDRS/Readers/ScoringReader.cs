using UnityEngine;

public class ScoringReader : RDRSReaderBase
{
    public enum ScoringParameterType
    {
        RemainingTime = 0,
        TimeInLevel = 1,
        CurrentViewers = 2,
        CurrentRetainer = 3,
        TotalViewers = 4,
        RemainTimeForViewers = 5,
        BestViewers = 6,
        BestTime = 7,
        DeltaViewers = 8,
        DeltaTime = 9,
        LastViewers = 10,
        LastTime = 11
    }

    [SerializeField] private ScoringParameterType parameter;
    [SerializeField] private bool autoFromParent = true;
    [SerializeField] private bool findByDistance = false;
    [SerializeField] private int playerID = -1;
    [SerializeField] private float maxDistance = 20000.0f;
    
    private ScoringDetector scoringDetector;

    public override object GetValue()
    {
        if (this.findByDistance)
        {
            this.FindClosestByDistance();
        }
        else if (this.scoringDetector == null)
        {
            this.FindScoringDetector();
        }

        if (this.scoringDetector == null)
        {
            return null;
        }

        switch (this.parameter)
        {
            case ScoringParameterType.RemainingTime:
                return this.scoringDetector.GetRemainingTime();
            case ScoringParameterType.TimeInLevel:
                return this.scoringDetector.GetTimeInLevel();
            case ScoringParameterType.CurrentViewers:
                return this.scoringDetector.GetCurrentViewers();
            case ScoringParameterType.CurrentRetainer:
                return this.scoringDetector.GetCurrentRetainer();
            case ScoringParameterType.TotalViewers:
                return this.scoringDetector.GetTotalViewers();
            case ScoringParameterType.RemainTimeForViewers:
                return this.scoringDetector.GetRemainTimeForViewers();
            case ScoringParameterType.BestViewers:
                return this.scoringDetector.GetBestViewers() ?? 0;
            case ScoringParameterType.BestTime:
                return this.scoringDetector.GetBestTime() ?? 0f;
            case ScoringParameterType.DeltaViewers:
                return this.scoringDetector.GetDeltaViewers() ?? 0;
            case ScoringParameterType.DeltaTime:
                return this.scoringDetector.GetDeltaTime() ?? 0f;
            case ScoringParameterType.LastViewers:
                return this.scoringDetector.GetLastViewers() ?? 0;
            case ScoringParameterType.LastTime:
                return this.scoringDetector.GetLastTime() ?? 0f;
            default:
                return 0f;
        }
    }

    private void FindScoringDetector()
    {
        if (this.autoFromParent)
        {
            this.scoringDetector = GetComponentInParent<ScoringDetector>();
            if (this.scoringDetector != null)
            {
                return;
            }
        }

        if (this.playerID >= -1)  //Diferent in the final Game
        {
            this.scoringDetector = FindFirstObjectByType<ScoringDetector>();
        }
    }


    private void FindClosestByDistance()
    {
        CarParameters[] allCars = FindObjectsByType<CarParameters>(FindObjectsSortMode.None);
        if (allCars.Length == 0)
        {
            return;
        }

        Transform myTransform = this.transform;
        CarParameters closestCar = null;
        float closestSqrDist = this.maxDistance * this.maxDistance;

        foreach (CarParameters car in allCars)
        {
            float sqrDist = (car.transform.position - myTransform.position).sqrMagnitude;
            if (sqrDist < closestSqrDist)
            {
                closestSqrDist = sqrDist;
                closestCar = car;
            }
        }

        if (closestCar != null)
        {
            this.scoringDetector = closestCar.GetComponentInChildren<ScoringDetector>();
        }
    }
}
