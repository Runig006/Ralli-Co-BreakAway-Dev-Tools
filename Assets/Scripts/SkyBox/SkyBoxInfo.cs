using UnityEngine;

public class SkyBoxInfo : MonoBehaviour
{
	[Header("General")]
	[SerializeField] private float TransactionSeconds = 0.0f;
	[SerializeField] private bool forcedCarLight;
	
	[SerializeField][Range(0, 24.0f)] private float hour = 11.0f;
	
	[SerializeField] private Color TopColor = new Color(4,107,255);
	[SerializeField] private Color MiddleColor = new Color(172,225,235);
	[SerializeField] private Color BottomColor = new Color(3,72,102);
	
	[SerializeField] private float TopExponent = 1.0f;
	[SerializeField] private float BottomExponent = 1.0f;
	
	[SerializeField] private bool sunEnabled = true;
	[SerializeField] private bool moonEnabled = true;
	[SerializeField] private bool starsEnabled = true;
	[SerializeField] private bool cloudsEnabled = true;

	[Header("Sun / Details")]
	[SerializeField] private float sunSize = 1.0f;
	[SerializeField] private float sunHalo = 1.0f;
	[SerializeField] private Color sunTintColor = new Color(192,192,192);
	[SerializeField] private Color sunLightColor = new Color(156,195,211);
	[SerializeField] private float sunLightIntensity = 1.0f;
	[SerializeField] private float sunAltitude = 45f;
	[SerializeField] private float sunLongitude = 0f;
	[SerializeField] private bool sunFlare = true;
	[SerializeField] private float sunFlareIntensity = 1.0f;

	[Header("Moon / Details")]
	[SerializeField] private float moonSize = 1.0f;
	[SerializeField] private float moonHalo = 1.0f;
	[SerializeField] private Color moonTintColor = new Color(125,141,155);
	[SerializeField] private Color moonLightColor = new Color(3,111,200);
	[SerializeField] private float moonLightIntensity = 1.0f;
	[SerializeField] private float moonAltitude = 45f;
	[SerializeField] private float moonLongitude = 0f;
	[SerializeField] private bool moonFlare = true;
	[SerializeField] private float moonFlareIntensity = 1.0f;

	[Header("Advanced")]
	[SerializeField] private Cubemap CloudsCubemap = null;
	[SerializeField] private Cubemap starsCubemap = null;
	
	private float timer = 0.0f;
	
	public void Start()
	{
	}

	// Getters
	public float GetTransactionSeconds()
	{
		return TransactionSeconds;
	}

	public Color GetTopColor()
	{
		return TopColor;
	}

	public Color GetMiddleColor()
	{
		return MiddleColor;
	}

	public Color GetBottomColor()
	{
		return BottomColor;
	}
	
	public float GetTopExponent()
	{
		return this.TopExponent;
	}
	
	public float GetBottomExponent()
	{
		return this.BottomExponent;
	}

	public bool GetForcedCarLight()
	{
		return forcedCarLight;
	}

	public float GetHour()
	{
		return hour;
	}

	public bool GetSunEnabled()
	{
		return sunEnabled;
	}

	public bool GetMoonEnabled()
	{
		return moonEnabled;
	}

	public bool GetStarsEnabled()
	{
		return starsEnabled;
	}

	public bool GetCloudsEnabled()
	{
		return cloudsEnabled;
	}

	public float GetSunSize()
	{
		return sunSize;
	}

	public float GetSunHalo()
	{
		return sunHalo;
	}
	
	public Color GetSunTintColor()
	{
		return sunTintColor;
	}
	
	public Color GetSunLightColor()
	{
		return sunLightColor;
	}
	
	public float GetSunLightIntensity()
	{
		return sunLightIntensity;
	}

	public float GetSunAltitude()
	{
		return sunAltitude;
	}

	public float GetSunLongitude()
	{
		return sunLongitude;
	}

	public bool GetSunFlare()
	{
		return sunFlare;
	}

	public float GetSunFlareIntensity()
	{
		return sunFlareIntensity;
	}

	public float GetMoonSize()
	{
		return moonSize;
	}

	public float GetMoonHalo()
	{
		return moonHalo;
	}
	
	public Color GetMoonTintColor()
	{
		return moonTintColor;
	}
	
	public Color GetMoonLightColor()
	{
		return moonLightColor;
	}
	
	public float GetMoonLightIntensity()
	{
		return moonLightIntensity;
	}

	public float GetMoonAltitude()
	{
		return moonAltitude;
	}

	public float GetMoonLongitude()
	{
		return moonLongitude;
	}

	public bool GetMoonFlare()
	{
		return moonFlare;
	}

	public float GetMoonFlareIntensity()
	{
		return moonFlareIntensity;
	}

	public Cubemap GetCloudsCubemap()
	{
		return CloudsCubemap;
	}

	public Cubemap GetStarsCubemap()
	{
		return starsCubemap;
	}
	
	
	
	void OnTriggerEnter(Collider collider)
	{
		
	}
}
