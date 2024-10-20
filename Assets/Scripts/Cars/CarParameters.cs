using System;
using System.Collections.Generic;
using UnityEngine;

public enum EngineSoundPrefabs
	{
		SmallTurboEngine = 0,
	}

public class CarParameters : MonoBehaviour
{
	[Header("Info")]
	[SerializeField] private string carName;
	[SerializeField] private Sprite photo;
	[Range(0,5)][SerializeField] private int difficulty;
	[Range(0,5)][SerializeField] private int acceleration;
	[Range(0,5)][SerializeField] private int topSpeed;
	[Range(0,5)][SerializeField] private int handling;
	
	[Header("Power")]
	[Tooltip("In M/S...because unity by default work that way")]
	[SerializeField] private float maxSpeed = 70.0f;
	[SerializeField] private float power = 2450.0f;
	[SerializeField] private float[] gears;
	[SerializeField] private float clutchDelay;
	
	[Header("SlipStream")]
	[SerializeField] private float maxSpeedSlip = 75.0f;
	[SerializeField] private float slipMultiplayer = 1.2f; 
	[SerializeField] private float slipStreamDistance = 200.00f;
	
	[Header("Boost")]
	[SerializeField] private float maxSpeedBoost = 85.0f;
	[SerializeField] private float boostMultiplayer = 3.0f;
	[SerializeField] private float boostAmmount = 10.0f;
	[SerializeField] private float boostRecoverSpeed = 0.5f;
	
	[Header("Turn")]
	[SerializeField] private float maxLateralSpeed = 12.0f;
	[SerializeField] private float maxTurnAngle = 45.0f;
	[SerializeField] private float downforce = 3.0f;
	
	[Header("Brake")]
	[SerializeField] private float brakePower = 750.0f;
	[SerializeField] private float dragBrake = 250.0f;
	
	[Header("Curves")]
	[SerializeField] private AnimationCurve engineCurve;
	[SerializeField] private AnimationCurve frontGrip;
	[SerializeField] private AnimationCurve backGrip;
	
	[Header("Terrains (Less score more 'Off-road')")]
	[Range(0.0f,1.0f)][SerializeField] private float powerStep = 1.0f;
	[Range(0.0f,1.0f)][SerializeField] private float gripStep = 1.0f;
	[Range(0.0f,1.0f)][SerializeField] private float backGripStep = 1.0f;
	
	[Header("Brake (Visual)")]
	[SerializeField] private float brakeStrongThreshold = 0.35f;
	
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
	
	private Transform centermass;
	private LayerMask layerMaskDrivable;

	//InputValues
	private int currentGear = 0;
	private float inputThrottleValue = 0.0f;
	private float inputBrakeValue = 0.0f;
	private float inputTurnValue = 0.0f;
	private bool inputBoosting = false;
	
	//Brake
	private float brakingStrongTime = 0.0f;
	
	//Suspension
	private Rigidbody carBody;
	private SuspensionPhysic[] suspensions;
	
	//BoostAvaiable
	private float boostAvailable = 0.0f;
	private float clutchTimer;
	
	//EasyCalculate RPM
	private int RPMMultiplier;
	
	//Spipstream
	private float slipstreamFactor = 1.0f;
	private int slipStreamMask;
	private bool isSlipstreaming = false;
	
	void Awake()
	{
		this.suspensions = GetComponentsInChildren<SuspensionPhysic>();
		this.carBody = GetComponent<Rigidbody>();
		this.boostAvailable = this.boostAmmount;
		
		this.centermass =  transform.Find("Body").Find("CenterOfMass").transform;
		this.carBody.centerOfMass = this.centermass.localPosition;
		
		this.RPMMultiplier = this.maxRPM - this.minRPM;
		
		this.layerMaskDrivable = LayerMask.GetMask("Track");
		this.slipStreamMask = LayerMask.GetMask("Trafic");
	}
	
	void FixedUpdate()
	{
		carBody.AddForce(-Vector3.up * this.downforce, ForceMode.Acceleration);
	
		this.UpdateBoost();
		this.UpdateClutch();
		this.UpdateBrakeTime();
		this.CheckForSlipstream();
	}
	
	private void UpdateBoost()
	{
		if(this.inputBoosting)
		{
			this.boostAvailable -= 1 * Time.deltaTime;
			if(this.boostAvailable < 0)
			{
				this.boostAvailable = 0;
			}
		}
		else
		{
			this.boostAvailable += this.boostRecoverSpeed * Time.deltaTime;
			if(this.boostAvailable > this.boostAmmount)
			{
				this.boostAvailable = this.boostAmmount;
			}
		}
	}

	private void UpdateClutch()
	{
		if(this.clutchTimer > 0)
		{
			this.clutchTimer -= Time.deltaTime;
		}
	}
	
	private void UpdateBrakeTime()
	{
		if (this.inputBrakeValue > this.brakeStrongThreshold && this.GetVelocityNormalice() > 0.1f)
		{
			this.brakingStrongTime += Time.deltaTime;
		}else
		{
			this.brakingStrongTime = 0.0f;
		}
	}
	
	private void CheckForSlipstream()
	{
		Collider[] hits;
		bool isSlipstreamDetected = false;
		float maxAngle = 8.0f;
		float minAngle = 1.0f;
		
		Vector3 forwardDirection = this.transform.forward;		
		if (this.inputThrottleValue < 0.2f)
		{
			this.isSlipstreaming = false;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, 1.0f, Time.deltaTime * 4.0f);
			return;
		}

		hits = Physics.OverlapSphere(this.transform.position, this.slipStreamDistance, this.slipStreamMask);
		foreach (Collider hit in hits)
		{
			Vector3 directionToHit = hit.transform.position - this.transform.position;
						
			float distance = Vector3.Distance(this.transform.position, hit.transform.position);
			float dynamicAngle = Mathf.Lerp(maxAngle, minAngle, distance / this.slipStreamDistance);
			float angle = Vector3.Angle(forwardDirection, directionToHit);
			if (angle <= dynamicAngle)
			{
				isSlipstreamDetected = true;
				break;
			}
		}
		
		if (isSlipstreamDetected)
		{
			this.isSlipstreaming = true;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, this.slipMultiplayer, Time.deltaTime * 3.0f);
		}
		else
		{
			this.isSlipstreaming = false;
			this.slipstreamFactor = Mathf.Lerp(this.slipstreamFactor, 1.0f, Time.deltaTime * 2.0f);
		}
	}	
	
	//UI
	public string GetCarName()
	{
		return this.carName;
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
		if(this.GetIsGearChanging())
		{
			return 0;
		}
		return this.inputThrottleValue;
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
		return this.inputBoosting && this.boostAvailable > 0;
	}

	public float GetBoostMultiplayer()
	{
		return this.boostMultiplayer;
	}

	public float GetBoostingNormalice()
	{
		return Mathf.Clamp(this.boostAvailable / this.boostAmmount,0,1);
	}

	// Gears
	public bool GetIsGearChanging()
	{
		return this.clutchTimer > 0.0f;
	}

	public int GetCurrentGear()
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

	public float GetGearRatio()
	{
		if(this.GetIsInReverse())
		{
			return this.gears[0];
		}
		return this.gears[this.currentGear];
	}

	public bool SetGear(int gear)
	{
		if(gear > this.gears.Length - 1 || gear < -1)
		{
			return false;
		}
		this.currentGear = gear;
		RestartClutch();
		return true;
	}

	private void RestartClutch()
	{
		if(this.clutchTimer <= 0)
		{
			this.clutchTimer = this.clutchDelay;
		}
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

	// Parameters
	public float GetMaxSpeed()
	{
		return this.maxSpeed;
	}

	public float GetMaxLateralSpeed()
	{
		return this.maxLateralSpeed;
	}
	
	public float GetMaxTurnAngle()
	{
		return this.maxTurnAngle;
	}

	public float GetPower()
	{
		float power = this.power;
		if (this.GetIsBoosting())
		{
			power *= this.boostMultiplayer;
		}
		power *= this.slipstreamFactor;
		return power;
	}

	public float GetBrakePower()
	{
		return this.brakePower;
	}

	public float GetDragBrake()
	{
		return this.dragBrake;
	}

	public AnimationCurve GetEngineCurve()
	{
		return this.engineCurve;
	}

	public AnimationCurve GetGripCurve(bool back)
	{
		return back ? this.backGrip : this.frontGrip;
	}

	// RPM
	public float GetMaxRPM()
	{
		return this.maxRPM;
	}

	public float GetFakeRPM()
	{
		return this.minRPM + (this.GetRPMNormalice() * this.RPMMultiplier);
	}

	public float GetRPMNormalice(bool takeAccountBoost = false)
	{
		float maxSpeed = this.maxSpeed;
		if(takeAccountBoost)
		{
			if(this.isSlipstreaming && maxSpeed < this.maxSpeedSlip)
			{
				maxSpeed = this.maxSpeedSlip;
			}
			if(this.GetIsBoosting() && maxSpeed < this.maxSpeedBoost)
			{
				maxSpeed = this.maxSpeedBoost;
			}
		}
		float rpmNormalize = Mathf.Abs(this.GetForwardVelocity()) * this.GetGearRatio() / maxSpeed;
		return rpmNormalize;
	}
	
	public float GetRPMDangerThresholdPercentage()
	{
		return this.rpmDangerThresholdPercentage;
	}
	
	//Terrain
	public float GetPowerStep()
	{
		return this.powerStep;
	}
	
	public float GetGripStep()
	{
		return this.gripStep;
	}
	
	public float GetBackGripStep()
	{
		return this.backGripStep;
	}

	// Calculated
	public float GetForwardVelocity()
	{
		return Vector3.Dot(this.carBody.linearVelocity, this.carBody.transform.forward);
	}

	public float GetVelocityNormalice()
	{
		return Mathf.Clamp(this.GetForwardVelocity() / this.GetMaxSpeed(), 0,1);
	}

	public float GetTorque()
	{
		float rpmNormalize = GetRPMNormalice(this.GetCurrentGear() == this.GetMaxGear());
		float torque = this.GetEngineCurve().Evaluate(rpmNormalize);
		if(this.GetIsInReverse())
		{
			torque *= -1;
		}
		return torque * this.GetPower();
	}

	public float GetEngineBrake()
	{
		float rpmPer = GetRPMNormalice();
		return this.dragBrake * this.GetGearRatio() * rpmPer;
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
}