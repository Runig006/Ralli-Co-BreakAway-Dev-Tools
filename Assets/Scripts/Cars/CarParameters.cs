using System;
using UnityEngine;

public enum EngineSoundPrefabs
{
	SmallTurboEngine = 0,
	AmericanV8Engine = 1,
	TwoLiterTurboEngine = 2,
}

public class CarParameters : MonoBehaviour
{
	[Header("Info")]
	[SerializeField] private string carName;
	[SerializeField] private string brandName;
	[SerializeField] private Sprite photo;
	[Range(0, 5)][SerializeField] private int difficulty;
	[Range(0, 5)][SerializeField] private int acceleration;
	[Range(0, 5)][SerializeField] private int topSpeed;
	[Range(0, 5)][SerializeField] private int handling;
	[Range(0, 5)][SerializeField] private int offRoad;
	
	
	[Header("Physic")]
	[SerializeField] Transform centermass;
	
	[Header("Power")]
	[Tooltip("In M/S...because unity by default work that way")]
	[SerializeField] private float maxSpeed = 70.0f;
	[SerializeField] private float power = 2450.0f;
	[SerializeField] private AnimationCurve engineCurve;
	
	[Header("Gears")]
	[SerializeField] private GearsMode gearsDefaultMode = GearsMode.Secuencial;
	[SerializeField] private ClutchMode clutchMode = ClutchMode.Automatic;
	[SerializeField] private float[] gears;
	[SerializeField] private float clutchDelay;
	
	[Header("SlipStream")]
	[SerializeField] private float maxSpeedSlip = 75.0f;
	[SerializeField] private float slipMultiplayer = 1.2f; 
	[SerializeField] private float slipStreamDistance = 350.00f;
	[SerializeField] private float slipstreamDuration = 1.5f;
	
	[Header("DownForce")]
	[SerializeField] private AnimationCurve downforceCurve;
	
	[Header("Boost")]
	[SerializeField] private float maxSpeedBoost = 85.0f;
	[SerializeField] private float boostMultiplayer = 3.0f;
	[SerializeField] private float boostAmmountSpeed = 10.0f;
	[SerializeField] private float boostRecoverSpeed = 0.5f;
	
	[Header("Brake")]
	[SerializeField] private float brakePower = 750.0f;
	
	[Header("Drag")]
	[Tooltip("Used as a 'Engine Brake' when the boost is burned, the player is off the throttle")]
	[SerializeField] private float maxDrag = 0.2f;
	[Tooltip("Since the cars I made has angular damping to 0.2, the crash are a little 'crazy'")]
	[SerializeField] private float crashMaxAngularDrag = 5.0f;
	
	[Header("Sound")]
	[SerializeField] private EngineSoundPrefabs engineSoundPrefab;
	[SerializeField] private float pitchSoundLevel = 1.0f;
	[SerializeField] private float pitchSoundLevelBoost = 1.5f;
	[SerializeField] private float aggressiveSoundLevel = 0.5f;
	[SerializeField] private float aggressiveSoundLevelBoost = 1.0f;
	
	[Header("UI")]
	[SerializeField] private int minRPM;
	[SerializeField] private int maxRPM;
	[SerializeField] private float rpmDangerThresholdPercentage = 0.8f;
	private LayerMask layerMaskDrivable;
	
	[Header("Debug")]
	public bool lightEnables = true;

	//InputValues
	private int? currentGear = null;
	private float inputThrottleValue = 0.0f;
	private float inputClutchValue = 0.0f;
	private float inputBrakeValue = 0.0f;
	private float inputTurnValue = 0.0f;
	private bool inputBoosting = false;	
	
	//Brake
	private float brakingStrongTime = 0.0f;
	
	//Suspension
	private Rigidbody carBody;
	private SuspensionPhysic[] suspensions;
	
	//BoostAvaiable
	private float boostTemperature = 0.0f;
	private bool forcedBoostBurned = false;
	private bool boostBurned = false;
	private float burnCheckTimer = 0.0f;
	private int? previousGear = null;
	private bool isDownshifting = false;
	private TemperatureZone currentTemperatureZone = null;
	
	//EasyCalculate RPM
	private int RPMMultiplier;
	private float simulatedRPM = 0.0f;
	private float clutchThreshold = 0.6f;
	
	//Spipstream
	private float slipstreamFactor = 1.0f;
	private int slipStreamMask;
	private bool isSlipstreaming = false;
	private float slipstreamTimer = 0.0f;
	
	//Drag
	private float originalDrag;
	private float originalAngularDrag;
	
	//PlayerControls
	private int playerId = 0;
	
	void Awake()
	{
		this.suspensions = GetComponentsInChildren<SuspensionPhysic>();
		this.carBody = GetComponent<Rigidbody>();
		
		this.originalDrag = this.carBody.linearDamping;
		this.originalAngularDrag = this.carBody.angularDamping;
		
		this.carBody.centerOfMass = this.centermass.localPosition;
		
		this.RPMMultiplier = this.maxRPM - this.minRPM;
		
		this.layerMaskDrivable = LayerMask.GetMask("Track");
		this.slipStreamMask = LayerMask.GetMask("Trafic");
	}
	
	void FixedUpdate()
	{
		this.carBody.centerOfMass = this.centermass.localPosition; //This is only in the "Debug Project"
		carBody.AddForce(-Vector3.up * this.downforceCurve.Evaluate(this.GetVelocityNormalize()), ForceMode.Acceleration);
	
		this.UpdateRPM();
	
		this.UpdateBrakeTime();
	
		this.UpdateBoost();
		this.CheckForSlipstream();
		this.UpdateDrag();
	}
	
	private void UpdateRPM()
	{
		float maxSpeed = this.maxSpeed;
		float rpmChangeRate = this.inputThrottleValue > 0 ? 0.3f : 0.05f;
		float clutchLerpFactor = Mathf.Clamp01((this.inputClutchValue - this.clutchThreshold) / (1 - this.clutchThreshold));

		if (this.inputClutchValue > this.clutchThreshold)
		{
			this.simulatedRPM = Mathf.Lerp(this.simulatedRPM, this.inputThrottleValue, Time.deltaTime * rpmChangeRate);
		}
		else
		{
			float targetRPM = Mathf.Abs(this.GetForwardVelocity()) * this.GetGearRatio() / maxSpeed;
			this.simulatedRPM = Mathf.Lerp(this.simulatedRPM, targetRPM, Time.deltaTime * (7 - clutchLerpFactor));
		}
	}
	
	private void UpdateBoost()
	{
		float coolingFactor = this.currentTemperatureZone?.GetCoolingMultiplier() ?? 1.0f;
		float heatingFactor = this.currentTemperatureZone?.GetHeatingMultiplier() ?? 1.0f;

		if(this.currentTemperatureZone != null && this.currentTemperatureZone.GetAutoTemperature() > 0.0f)
		{
		    this.boostTemperature += this.currentTemperatureZone.GetAutoTemperature() * Time.deltaTime;
		    this.boostTemperature = MathF.Max(this.boostTemperature, 0);
		    this.boostTemperature = MathF.Min(this.boostTemperature, 200);
		}
		
		if(this.inputBoosting && this.boostBurned == false)
		{
			this.boostTemperature += this.boostAmmountSpeed * heatingFactor * Time.deltaTime;
			this.boostTemperature = MathF.Min(this.boostTemperature, 200);
			
			if(this.boostTemperature > 100)
			{
				this.burnCheckTimer += Time.deltaTime;
				if (burnCheckTimer >= 1.0f) 
				{
					float excess = this.boostTemperature - 100;
					this.burnCheckTimer = 0.0f; 

					float chance = Mathf.Clamp01(excess / 100.0f); 
					if (UnityEngine.Random.value < chance)
					{
						this.boostBurned = true;
					}
				}
			}
		}
		
		else if(this.GetRPMNormalize() <= 1.10f)
		{
			float reduce = this.boostRecoverSpeed * coolingFactor * Time.deltaTime;
			if(this.boostBurned)
			{
				reduce *= 0.75f;
			}
			this.boostTemperature -= reduce;
			this.boostTemperature = MathF.Max(this.boostTemperature, 0);
			if (this.boostBurned && this.boostTemperature < 100)
			{
				this.boostBurned = false;
			}
			this.isDownshifting = false;
		}
		
		if(this.inputClutchValue < 0.8f && this.currentGear != null){
			if ( this.currentGear < this.previousGear)
			{
				this.isDownshifting = true;
			}
			if (this.GetRPMNormalize() >= 1.00f)
			{
				if (this.isDownshifting && this.boostTemperature <= 250.0f)
				{
					this.boostTemperature += (this.GetRPMNormalize() - 1.0f) * 60 * Time.deltaTime;
				}
			}
			else
			{
				this.previousGear = this.currentGear ?? 0;
			}
		}
	}
	
	private void UpdateBrakeTime()
	{
		//Check if is braking "hard"
		if (this.inputBrakeValue > 0.35f && this.GetVelocityNormalize() > 0.1f)
		{
			this.brakingStrongTime += Time.deltaTime;
		}else
		{
			this.brakingStrongTime = 0.0f;
		}
	}
	
	private void CheckForSlipstream()
	{
		//If is not in the throttle stop the slipstreaming
		if (this.inputThrottleValue < 0.2f)
		{
			this.isSlipstreaming = false;
			this.slipstreamTimer = 0;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, 1.0f, Time.deltaTime * 4.0f);
			return;
		}
		
		//Check if is detected, if it is, restart the timer
		if (this.DetectSlipstream())
		{
			this.isSlipstreaming = true;
			this.slipstreamTimer = this.slipstreamDuration;
		}
		
		if (this.slipstreamTimer > 0)
		{
			this.slipstreamTimer -= Time.deltaTime;
			this.isSlipstreaming = true;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, this.slipMultiplayer, Time.deltaTime * 3.0f);
		}
		
		else
		{
			this.isSlipstreaming = false;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, 1.0f, Time.deltaTime * 2.0f);
		}
	}
	
	private bool DetectSlipstream()
	{
		Collider[] hits = Physics.OverlapSphere(this.transform.position, this.slipStreamDistance, this.slipStreamMask);
		Vector3 forwardDirection = this.transform.forward;
		float maxAngle = 8.0f;
		float minAngle = 1.0f;

		foreach (Collider hit in hits)
		{
			Vector3 directionToHit = hit.transform.position - this.transform.position;
			float distance = Vector3.Distance(this.transform.position, hit.transform.position);
			float dynamicAngle = Mathf.Lerp(maxAngle, minAngle, distance / this.slipStreamDistance);
			float angle = Vector3.Angle(forwardDirection, directionToHit);

			if (angle <= dynamicAngle)
			{
				return true;
			}
		}
		return false;
	}
	
	private void UpdateDrag()
	{
		float drag = this.originalDrag;
		
		if (this.inputThrottleValue <= 0.1f || this.GetRPMNormalize() > 1.1f)
		{
			drag = this.maxDrag * Mathf.Clamp01(this.GetRPMNormalize() - 0.1f);
		}
		
		if(this.boostBurned)
		{
			drag = this.maxDrag;
		}
		this.carBody.linearDamping = drag;
		
		//Angular
		if(this.carBody.angularDamping != this.originalAngularDrag){
			this.carBody.angularDamping = Mathf.MoveTowards(this.carBody.angularDamping, this.originalAngularDrag, Time.deltaTime * (this.crashMaxAngularDrag - this.originalAngularDrag));
		}
	}
	
	//Normaly, i put the gets at the start...but there are too fucking many of them...
	//PlayerID
	public void SetPlayerId(int playerId)
	{
		this.playerId = playerId;
	}
	
	public int GetPlayerId()
	{
		return this.playerId;
	}
	
	
	//UI
	public string GetCarName()
	{
		return this.carName;
	}
	
	public string GetBrandName()
	{
		return this.brandName;
	}
	
	public Sprite GetPhoto()
	{
		return this.photo;
	}

	public float GetDifficulty()
	{
		return this.difficulty;
	}

	public int GetAcceleration()
	{
		return this.acceleration;
	}

	public int GetTopSpeed()
	{
		return this.topSpeed;
	}

	public int GetHandling()
	{
		return this.handling;
	}
	
	public int GetOffRoad()
	{
		return this.offRoad;
	}

	// Globals
	public Rigidbody GetCarBody()
	{
		return this.carBody;
	}

	public LayerMask GetMaskDrivable()
	{
		return this.layerMaskDrivable;
	}

	// Controls - Setters
	public void SetThrottle(float value)
	{
		value = Mathf.Clamp(value, 0, 1);
		this.inputThrottleValue = value;
	}
	
	public void SetClutch(float value)
	{
		value = Mathf.Clamp(value, 0,1);
		this.inputClutchValue = value;
	}

	public void SetBrake(float value)
	{
		value = Mathf.Clamp(value, 0, 1);
		this.inputBrakeValue = value;
	}

	public void SetTurn(float value)
	{
		value = Mathf.Clamp(value, -1, 1);
		this.inputTurnValue = value;
	}

	public void SetBoosting(bool value)
	{
		this.inputBoosting = value;
	}

	// Controls - Getters
	public float GetThrottle()
	{
		return this.inputThrottleValue;
	}
	
	public float GetClutch()
	{
		return this.inputClutchValue;
	}

	public float GetBrake()
	{
		return this.inputBrakeValue;
	}

	public float GetTurn()
	{
		return this.inputTurnValue;
	}

	// Boost
	public bool GetIsBoosting()
	{
		return this.inputBoosting && this.boostBurned == false;
	}

	public float GetBoostMultiplayer()
	{
		return this.boostMultiplayer;
	}
	
	public float GetBoostTemperature()
	{
		return this.boostTemperature;
	}
	
	public bool GetBoostBurn()
	{
		return this.boostBurned;
	}
	
	public TemperatureZone? GetTemperatureZone()
	{
		return this.currentTemperatureZone;
	}
	
	public void SetTemperatureZone(TemperatureZone zone)
	{
		this.currentTemperatureZone = zone;
	}

	// Gears
	public int? GetCurrentGear()
	{
		return this.currentGear;
	}

	public int GetMaxGear()
	{
		return this.gears.Length - 1;
	}

	public bool GetIsInReverse()
	{
		return this.currentGear == -1;
	}
	
	public float GetClutchDelay()
	{
		return this.clutchDelay;
	}
	
	public GearsMode GetGearsMode()
	{
		return this.gearsDefaultMode;
	}
	
	public ClutchMode GetClutchMode()
	{
		return this.clutchMode;
	}
	
	public float[] GetGears()
	{
		return this.gears;
	}

	public float GetGearRatio()
	{
		if(this.currentGear == null)
		{
			return 0.0f;
		}
		if(this.GetIsInReverse())
		{
			return this.gears[0];
		}
		return this.gears[this.currentGear ?? 0];
	}

	public bool SetGear(int? gear, bool force = false)
	{
		if(gear > this.gears.Length - 1 || gear < -1)
		{
			return false;
		}
		if(force == false && this.inputClutchValue <= 0.8f)
		{
			//this.currentGear = null;
			//return false;
		}
		this.currentGear = gear;
		return true;
	}
	
	//Brake
	public float GetBrakeStrongTimer()
	{
		return this.brakingStrongTime;
	}

	// Road
	public bool GetGrounded()
	{
		foreach(SuspensionPhysic su in this.suspensions)
		{
			if(su.GetGrounded())
			{
				return true;
			}
		}
		return false;
	}
	
	public SuspensionPhysic[] GetSuspensions()
	{
		return this.suspensions;
	}

	// Parameters
	public float GetMaxSpeed()
	{
		return this.maxSpeed;
	}

	public float GetPower()
	{
		float power = this.power;
		if (this.GetIsBoosting())
		{
			power *= this.boostMultiplayer;
		}
		power *= this.slipstreamFactor;
		if (this.boostBurned == true)
		{
			power = this.power * 0.75f;
		}
		return power;
	}

	public float GetBrakePower()
	{
		return this.brakePower;
	}

	public AnimationCurve GetEngineCurve()
	{
		return this.engineCurve;
	}

	// RPM
	public float GetMaxRPM()
	{
		return this.maxRPM;
	}

	public float GetFakeRPM()
	{
		return this.minRPM + (this.GetRPMNormalize() * this.RPMMultiplier);
	}

	public float GetRPMNormalize(bool direct = false)
	{
		return direct ?  Mathf.Abs(this.GetForwardVelocity()) * this.GetGearRatio() / maxSpeed : this.simulatedRPM;
	}
		
	public float GetRPMDangerThresholdPercentage()
	{
		return this.rpmDangerThresholdPercentage;
	}

	// Calculated
	public float GetForwardVelocity()
	{
		if(this.carBody == null)
		{
			return 0.0f; //Because Unity call before the OnEnable...yeah
		}
		return Vector3.Dot(this.carBody.linearVelocity, this.carBody.transform.forward);
	}
	
	public float GetVelocityTraslated()
	{
		return this.GetForwardVelocity() * 3.6f; //In the good game will be traslated to KM/H or MP/H
	}

	public float GetVelocityNormalize()
	{
		return Mathf.Clamp(this.GetForwardVelocity() / this.GetMaxSpeed(), 0,1);
	}

	public float GetTorque()
	{
		float rpmNormalize = this.GetRPMNormalize();
		float torque = this.GetEngineCurve().Evaluate(rpmNormalize);
		
		if (
			this.ReachedMaxSpeed() ||
			this.currentGear == null || 
			this.inputClutchValue > 0.8f
			)
		{
			return 0.0f;
		}
		
		if(this.GetIsInReverse())
		{
			torque *= -1;
		}
		return torque * this.GetPower() * (1 - this.inputClutchValue);
	}
	
	private bool ReachedMaxSpeed()
	{
		if (this.currentGear == null) 
		{
			return true;
		}

   		float maxAllowedSpeed = this.maxSpeed / this.GetGearRatio();
		if (this.GetCurrentGear() == this.GetMaxGear())
		{
			if (this.isSlipstreaming && maxAllowedSpeed < this.maxSpeedSlip)
			{
				maxAllowedSpeed = this.maxSpeedSlip;
			}
			if (this.GetIsBoosting() && maxAllowedSpeed < this.maxSpeedBoost)
			{
				maxAllowedSpeed = this.maxSpeedBoost;
			}
		}
		return this.GetForwardVelocity() >= maxAllowedSpeed;
	}
	
	//Audio	
	public int GetEngineSoundPrefab()
	{
		return (int) this.engineSoundPrefab;
	}
	
	public float GetPitchSoundLevel()
	{
		return this.pitchSoundLevel;
	}
	
	public float GetPitchSoundLevelBoost()
	{
		return this.pitchSoundLevelBoost;
	}
	
	public float GetAggressiveSoundLevel()
	{
		return this.aggressiveSoundLevel;
	}
	
	public float GetAggressiveSoundLevelBoost()
	{
		return this.aggressiveSoundLevelBoost;
	}
	
	private void OnCollisionEnter(Collision collision)
	{
    	this.carBody.angularDamping = Mathf.Clamp(collision.relativeVelocity.magnitude * 0.1f, this.originalAngularDrag, this.crashMaxAngularDrag);
	}
}