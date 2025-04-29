using UnityEngine;

public class SunMoonDebugger : MonoBehaviour
{
    [SerializeField] private SkyBoxInfo skyboxInfo;
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private Transform currentLevelTransform;
    
    private float sunrise = 6f;
    private float sunset = 20f;
    private Vector2 sunOrbit = new Vector2(-20f, 200f);
    
    private float moonrise = 22f;
    private float moonset = 5f;
    private Vector2 moonOrbit = new Vector2(-20f, 200f);
    
    private Vector3 sunAttitudeVector;
    private Vector3 moonAttitudeVector;
    
    private float sunDuration;
    private float moonDuration;

    private void Update()
    {
        if (this.skyboxInfo == null) {
            return;
        }

        float time = this.skyboxInfo.GetHour();
        
        RecalculateAttitude();

        UpdateSun(time);
        UpdateMoon(time);
    }
    
    private void RecalculateAttitude()
    {
        this.sunDuration = (this.sunrise < this.sunset) ? this.sunset - this.sunrise : 24f - this.sunrise + this.sunset;
        float radSun = this.skyboxInfo.GetSunAltitude() * Mathf.Deg2Rad;
        this.sunAttitudeVector = new Vector3(Mathf.Sin(radSun), Mathf.Cos(radSun), 0f);

        this.moonDuration = (this.moonrise < this.moonset) ? this.moonset - this.moonrise : 24f - this.moonrise + this.moonset;
        float radMoon = this.skyboxInfo.GetMoonAltitude() * Mathf.Deg2Rad;
        this.moonAttitudeVector = new Vector3(Mathf.Sin(radMoon), Mathf.Cos(radMoon), 0f);
    }

    private void UpdateSun(float time)
    {
        if (this.sunLight == null || this.skyboxInfo.GetSunEnabled() == false) {
            return;
        }

        if (time > this.sunrise || time < this.sunset) {
            float sunCurrent = time - this.sunrise;
            if (sunCurrent < 0f) {
                sunCurrent += 24f;
            }

            float ty = sunCurrent / this.sunDuration;
            float dy = Mathf.Lerp(this.sunOrbit.x, this.sunOrbit.y, ty);

            Quaternion rotation = Quaternion.AngleAxis(this.skyboxInfo.GetSunLongitude() - 180f, Vector3.up) *
                                Quaternion.AngleAxis(dy, this.sunAttitudeVector);

            if (this.currentLevelTransform != null) {
                rotation = this.currentLevelTransform.rotation * rotation;
            }

            Vector3 euler = rotation.eulerAngles;
            euler.z = 0f;
            this.sunLight.transform.rotation = Quaternion.Euler(euler);

            this.sunLight.color = this.skyboxInfo.GetSunLightColor();
            this.sunLight.intensity = this.skyboxInfo.GetSunLightIntensity();
        }
    }

    private void UpdateMoon(float time)
    {
        if (this.moonLight == null || this.skyboxInfo.GetMoonEnabled() == false) {
            return;
        }

        if (time > this.moonrise || time < this.moonset) {
            float moonCurrent = time - this.moonrise;
            if (moonCurrent < 0f) {
                moonCurrent += 24f;
            }

            float ty = moonCurrent / this.moonDuration;
            float dy = Mathf.Lerp(this.moonOrbit.x, this.moonOrbit.y, ty);

            Quaternion rotation = Quaternion.AngleAxis(this.skyboxInfo.GetMoonLongitude() - 180f, Vector3.up) *
                                Quaternion.AngleAxis(dy, this.moonAttitudeVector);

            if (this.currentLevelTransform != null) {
                rotation = this.currentLevelTransform.rotation * rotation;
            }

            Vector3 euler = rotation.eulerAngles;
            euler.z = 0f;
            this.moonLight.transform.rotation = Quaternion.Euler(euler);

            this.moonLight.color = this.skyboxInfo.GetMoonLightColor();
            this.moonLight.intensity = this.skyboxInfo.GetMoonLightIntensity();
        }
    }

}
