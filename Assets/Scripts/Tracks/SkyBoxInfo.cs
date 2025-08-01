using UnityEngine;

public class SkyBoxInfo : MonoBehaviour
{
	[Header("General")]
	[SerializeField] private float TransactionSeconds = 0.0f;
	[SerializeField] private bool carLight;
	
	[SerializeField][Range(0, 24.0f)] private float hour = 11.0f;
	
	[SerializeField] private Color TopColor = new Color(0.02f, 0.42f, 1.00f);
	[SerializeField] private Color MiddleColor = new Color(0.67f, 0.88f, 0.92f);
	[SerializeField] private Color BottomColor = new Color(0.01f, 0.28f, 0.40f);
	
	[SerializeField] private float TopExponent = 1.0f;
	[SerializeField] private float BottomExponent = 1.0f;
	
	[SerializeField] private bool sunEnabled = true;
	[SerializeField] private bool moonEnabled = true;
	[SerializeField] private bool starsEnabled = true;
	[SerializeField] private bool cloudsEnabled = true;

	[Header("Sun")]
	[SerializeField] private float sunSize = 1.0f;
	[SerializeField] private float sunHalo = 1.0f;
	[SerializeField] private Color sunTintColor = new Color(0.75f, 0.75f, 0.75f);
	[SerializeField] private Color sunLightColor = new Color(0.61f, 0.76f, 0.83f);
	[SerializeField] private float sunLightIntensity = 1.0f;
	[SerializeField] private float sunAltitude = 45f;
	[SerializeField] private float sunLongitude = 0f;
	[SerializeField] private bool sunFlare = true;
	[SerializeField] private float sunFlareIntensity = 1.0f;

	[Header("Moon")]
	[SerializeField] private float moonSize = 1.0f;
	[SerializeField] private float moonHalo = 1.0f;
	[SerializeField] private Color moonTintColor = new Color(0.49f, 0.55f, 0.6f);
	[SerializeField] private Color moonLightColor = new Color(0.01f, 0.43f, 0.78f);
	[SerializeField] private float moonLightIntensity = 1.0f;
	[SerializeField] private float moonAltitude = 45f;
	[SerializeField] private float moonLongitude = 0f;
	[SerializeField] private bool moonFlare = true;
	[SerializeField] private float moonFlareIntensity = 1.0f;
	
	[Header("Clouds")]
	[SerializeField] private Color cloudTint = new Color(0.5f,0.5f,0.5f);
	[SerializeField] [Range(-0.75f, 0.75f)] private float cloudHeight = 0;
	[SerializeField] [Range(0, 360f)] private float cloudRotation = 0;
	
	
	[Header("Stars")]
	[SerializeField] private Color starsTint = new Color(0.5f,0.5f,0.5f);
	[SerializeField] [Range(0f, 10f)] private float starsExtinction = 2f;
	[SerializeField] [Range(0f, 25f)] private float starsTwinklingSpeed = 10f;
	
	[Header("Ambient Light")]
	[SerializeField] private bool overrideAmbientLight = false;
	[SerializeField] private Color ambientColor = new Color(0.255f, 0.255f, 0.255f, 1f);

	[Header("Fog")]
	[SerializeField] private bool overrideFog = false;
	[SerializeField] private FogMode fogMode = FogMode.Linear;
	[SerializeField] private Color fogColor = new Color(0.255f, 0.255f, 0.255f, 1f);
	[SerializeField] private float fogStartDistanceOrDensity = 0f;
	[SerializeField] private float fogEndDistance = 300f;

	[Header("Textures")]
	[SerializeField] private Texture2D sunTextureOverride = null;
	[SerializeField] private Texture2D moonTextureOverride = null;
	[SerializeField] private Cubemap cloudsCubemap = null;
	[SerializeField] private Cubemap starsCubemap = null;

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

	public bool GetCarLight()
	{
		return carLight;
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
	
	public Color GetCloudTint()
	{
	    return cloudTint;
	}
	
	public float GetCloudHeight()
	{
	    return cloudHeight;
	}
	
	public float GetCloudRotation()
	{
	    return cloudRotation;
	}
	
	public Color GetStarsTint()
	{
	    return starsTint;
	}
	
	public float GetStarsExtinction()
	{
	    return starsExtinction;
	}
	
	public float GetStarsTwinklingSpeed()
	{
	    return starsTwinklingSpeed;
	}

	public Texture2D GetSunTexture()
	{
		return this.sunTextureOverride;
	}
	
	public Texture2D GetMoonTexture()
	{
		return this.moonTextureOverride;
	}

	public Cubemap GetStarsCubemap()
	{
		return this.starsCubemap;
	}

	public Cubemap GetCloudsCubemap()
	{
		return this.cloudsCubemap;
	}
	
	public bool GetOverrideAmbientLight()
	{
		return this.overrideAmbientLight;
	}

	public Color GetAmbientColor()
	{
		return this.ambientColor;
	}

	public bool GetOverrideFog()
	{
		return this.overrideFog;
	}

	public FogMode GetFogMode()
	{
		return this.fogMode;
	}

	public Color GetFogColor()
	{
		return this.fogColor;
	}

	public float GetFogStartDistanceOrDensity()
	{
		return this.fogStartDistanceOrDensity;
	}

	public float GetFogEndDistance()
	{
		return this.fogEndDistance;
	}

}
