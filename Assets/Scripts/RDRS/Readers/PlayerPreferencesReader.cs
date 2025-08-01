using UnityEngine;

public class PlayerPreferencesReader : RDRSNode
{
    public enum PlayerPreferenceType
    {
        SpeedUnit = 0,
        SpeedMultiplier = 1,
        SpeedSuffix = 2,

        TemperatureUnit = 3,
        TemperatureMultiplier = 4,
        TemperatureSuffix = 5,

        CarColor = 6,
        CarColorSecundary = 7,
        CarColorTertiary = 8,
    }

    public enum SpeedUnit
    {
        MetersPerSecond = 0,
        KilometersPerHour = 1,
        MilesPerHour = 2
    }

    public enum TemperatureUnit
    {
        Celsius = 0,
        Fahrenheit = 1
    }

    [SerializeField] private PlayerPreferenceType preferenceType;

    /*This variables are for debug*/
    public static SpeedUnit preferredSpeedUnit = SpeedUnit.KilometersPerHour;
    public static TemperatureUnit preferredTemperatureUnit = TemperatureUnit.Celsius;
    public static Color preferredCarColor = Color.white;

    public override object GetValue()
    {
        switch (preferenceType)
        {
            case PlayerPreferenceType.SpeedUnit:
                return preferredSpeedUnit;

            case PlayerPreferenceType.SpeedMultiplier:
                return this.GetSpeedMultiplier(preferredSpeedUnit);

            case PlayerPreferenceType.SpeedSuffix:
                return this.GetSpeedSuffix(preferredSpeedUnit);

            case PlayerPreferenceType.TemperatureUnit:
                return preferredTemperatureUnit;

            case PlayerPreferenceType.TemperatureMultiplier:
                return this.GetTemperatureMultiplier(preferredTemperatureUnit);

            case PlayerPreferenceType.TemperatureSuffix:
                return this.GetTemperatureSuffix(preferredTemperatureUnit);

            case PlayerPreferenceType.CarColor:
                return preferredCarColor;

            default:
                return null;
        }
    }

    private float GetSpeedMultiplier(SpeedUnit unit)
    {
        switch (unit)
        {
            case SpeedUnit.MetersPerSecond:
                return 1f;
            case SpeedUnit.KilometersPerHour:
                return 3.6f;
            case SpeedUnit.MilesPerHour:
                return 2.23694f;
            default:
                return 1f;
        }
    }

    private string GetSpeedSuffix(SpeedUnit unit)
    {
        switch (unit)
        {
            case SpeedUnit.MetersPerSecond:
                return "m/s";
            case SpeedUnit.KilometersPerHour:
                return "km/h";
            case SpeedUnit.MilesPerHour:
                return "mph";
            default:
                return "";
        }
    }

    private float GetTemperatureMultiplier(TemperatureUnit unit)
    {
        switch (unit)
        {
            case TemperatureUnit.Celsius:
                return 1f;
            case TemperatureUnit.Fahrenheit:
                return 1.8f;
            default:
                return 1f;
        }
    }

    private string GetTemperatureSuffix(TemperatureUnit unit)
    {
        switch (unit)
        {
            case TemperatureUnit.Celsius:
                return "°C";
            case TemperatureUnit.Fahrenheit:
                return "°F";
            default:
                return "";
        }
    }
}
