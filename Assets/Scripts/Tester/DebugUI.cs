using TMPro;
using UnityEngine;

public class DebugUI : MonoBehaviour
{
	[SerializeField] private TMP_Text gear;
	[SerializeField] private TMP_Text speed;
	[SerializeField] private TMP_Text boost;
	[SerializeField] private TMP_Text rpm;
	
	private CarParameters carParameters;
	
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
	}

	// Update is called once per frame
	void Update()
	{
		this.gear.text = "Gear: " + (this.carParameters.GetCurrentGear() + 1);
		this.speed.text = "Speed: " + (this.carParameters.GetForwardVelocity() * 3.6f).ToString("F0");
		this.boost.text = "Boost: " +  (this.carParameters.GetBoostTemperature()).ToString("F0") + "°C";
		this.rpm.text = "RPM: " + this.carParameters.GetFakeRPM().ToString("F0");
	}
}
