using UnityEngine;

public class TemperatureZone : MonoBehaviour
{
    [SerializeField] private float coolingMultiplier = 1.0f;
    [SerializeField] private float heatingMultiplier = 1.0f;
    [SerializeField] private float autoTemperature = 0.0f;

    public float GetCoolingMultiplier()
    {
        return this.coolingMultiplier;
    }
    
    public float GetHeatingMultiplier()
    {
        return this.heatingMultiplier;
    }
    
    public float GetAutoTemperature()
    {
        return this.autoTemperature;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("PlayerRealBody") == false)
        {    
            return;
        }

        CarParameters car = collider.GetComponentInParent<CarParameters>();
        car.SetTemperatureZone(this);
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.CompareTag("PlayerRealBody") == false)
        {    
            return;
        }
    
        CarParameters car = collider.GetComponentInParent<CarParameters>();
        if (car.GetTemperatureZone() == this)
        {
            car.SetTemperatureZone(null);
        }
    }
}
