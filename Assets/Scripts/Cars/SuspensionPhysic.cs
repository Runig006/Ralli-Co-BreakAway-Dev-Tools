using System;
using UnityEngine;

public class SuspensionPhysic : MonoBehaviour
{	
	[Header("Engine Settings")]
	[Range(0,1)][SerializeField] private float powerMultiplayer = 0.0f;
	[Range(0,1)][SerializeField] private float powerTerrainInfluence = 0.0f;
	
	[Header("Diferencial")]
	[Range(0,5)][SerializeField] private float maxTorqueByDifferential = 2.0f;
	[Range(0,5)][SerializeField] private float minTorqueByDifferential = 0.0f;
	[SerializeField] private SuspensionPhysic oppositeWheel;
	
	[Header("Turn Settings")]
	[SerializeField] private bool turnWheel = true;
	[SerializeField] private float maxTurnAngle = 32.0f;
	
	[Header("Grip Settings")]
	[Range(0,1)][SerializeField] private float gripTerrainInfluence = 0.0f;
	[SerializeField] private float maxLateralSpeed = 36.0f;
	[SerializeField] private AnimationCurve gripCurve;
	
	[Header("Suspension Settings")]
	[SerializeField] private float springStiff = 1250.0f;
	[SerializeField] private float damping = 850.0f;	
	[SerializeField] private float lateralForceMultiplayer = 11.0f;
	
	[Header("Suspension Settings Height")]
	[SerializeField] private float springRestPosition = 0.15f;
	[SerializeField] private float springMaxTravel = 0.25f;
	[SerializeField] private float floorOffset = 0.4f;
	
	//Others
	private CarParameters carParameters;
	private Rigidbody carBody;
	
	private float grip;
	
	private float currentWheelRotation = 0;
	
	private bool isGrounded = true;
	private RaycastHit hitFloor;
	
	private float sideForce;
	private float sideForceNormalize;
	private int sideForceDirection;
	
	private float currentSuspensionForce;
	private float currentSpringLenght;
	private float normalizedSpringLength;
	
	private TerrainInfo currentTerrain;
	
	private bool driftEnable;
	private bool dustEnable;
		
	//GETTERS/SETTER
	public bool GetGrounded()
	{
		return this.isGrounded;
	}
	
	public bool GetTurnWheel()
	{
		return this.turnWheel;
	}
	
	public float GetCurrentTurnAngle()
	{
		if(this.carParameters == null)
		{
			return 0.0f; //...yeah...magic order of execution
		}
		return this.carParameters.GetTurn() * this.maxTurnAngle;
	}

	/* Is normalized between 0 and 1 */
	public float GetSideForce()
	{
		return this.sideForce;
	}
	
	public float GetSideForceNormalize()
	{
		return this.sideForceNormalize;
	}
	
	public float GetCurrentGrip()
	{
		return this.grip;
	}
	
	public int GetSideForceDirection()
	{
		return this.sideForceDirection;
	}
	
	public float GetCurrentSuspensionForce()
	{
		return this.currentSuspensionForce; 
	}

	public float GetCurrentSpringLength()
	{
		return this.currentSpringLenght;
	}
	
	public float GetCurrentSpringLengthNormalize()
	{
		return this.normalizedSpringLength;
	}
	
	//Terrain
	public TerrainInfo GetCurrentTerrain()
	{
		return this.currentTerrain;
	}
	
	public float GetPowerMultipler()
	{
		if(this.currentTerrain)
		{
			if(this.currentTerrain.GetPowerMultipler() > 1.0f) //If is MORE than normal...all cars should be affected
			{
			    return this.currentTerrain.GetPowerMultipler();
			}
			return Mathf.Lerp(1, this.currentTerrain.GetPowerMultipler(), this.powerTerrainInfluence);
		}else{
			return 1;
		}
	}
	
	public float GetResitanceMultipler()
	{
		if(this.currentTerrain)
		{
			return Mathf.Lerp(0, this.currentTerrain.GetResistanceMultipler(), this.powerTerrainInfluence);
		}else{
			return 0.0f;
		}
	}
	
	public float GetGripMultiplier()
	{
		if(this.currentTerrain)
		{
			if(this.currentTerrain.GetGripMultipler() > 1.0f) //If is MORE than normal...all cars should be affected
			{
			    return this.currentTerrain.GetGripMultipler();
			}
			return Mathf.Lerp(1, this.currentTerrain.GetGripMultipler(), this.gripTerrainInfluence);
		}else
		{
			return 1;
		}
	}
	
	//LOGIC
	void Start()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
		this.carBody = this.carParameters.GetCarBody();
	}
	
	void FixedUpdate()
	{	
		if(this.turnWheel)
		{
			this.TurnWheel();
		}
		
		this.CalculateSuspension();
		if(this.isGrounded)
		{
			this.CalculateRotation();
			this.CalculateSpeed();
		}
	}
	
	void CalculateSuspension()
	{
		float maxLength = this.springRestPosition + this.springMaxTravel + this.floorOffset;
		this.isGrounded = Physics.Raycast(this.transform.position, this.transform.up * -1, out this.hitFloor, maxLength , this.carParameters.GetMaskDrivable());

		float springLength = this.isGrounded ? this.hitFloor.distance - this.floorOffset : maxLength;
		springLength = Mathf.Clamp(springLength, 0f , springRestPosition + springMaxTravel);
		
		float springCompression = (this.springRestPosition - springLength) / this.springMaxTravel;
		
		float springVelocity = Vector3.Dot(this.carBody.GetPointVelocity(this.transform.position), this.transform.up);
		float netForce = (springCompression * this.springStiff) - (springVelocity * this.damping);
		
		this.currentSuspensionForce = netForce;
		this.currentSpringLenght = springLength;
		this.normalizedSpringLength = Mathf.InverseLerp(0.0f, this.springRestPosition + this.springMaxTravel, springLength);
			
		if(this.isGrounded)
		{
			this.carBody.AddForceAtPosition(netForce * this.transform.up, this.transform.position);
			this.currentTerrain = this.hitFloor.collider.GetComponent<TerrainInfo>();
		}
		else
		{
		    this.currentTerrain = null;
		}
	}
	
	void CalculateRotation()
	{
		//Get the force of the wheels going sideway and normalize between 0-1
		Vector3 steeringDir = this.transform.right;
		Vector3 tireWorldVel = this.carBody.GetPointVelocity(this.transform.position);
		float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
		float normalized = Mathf.Clamp01(Mathf.Abs(steeringVel) / this.maxLateralSpeed);
		
		//Set the variables for external scripts
		this.sideForce = Mathf.Abs(steeringVel);
		this.sideForceNormalize = normalized;
		this.sideForceDirection = steeringVel >= 0 ? -1 : 1;
		
		//The real logic
		float newGrip = this.gripCurve.Evaluate(normalized);
		this.grip = Mathf.Lerp(this.grip, newGrip, 0.5f);
		
		float desiredVelChange = -steeringVel * this.grip;
		float desirecAccel = desiredVelChange / Time.fixedDeltaTime;
		this.carBody.AddForceAtPosition(steeringDir * this.lateralForceMultiplayer * desirecAccel * this.GetGripMultiplier(), this.transform.position);
	}
	
	void CalculateSpeed()
	{
		if (this.carParameters.GetBrake() > 0.0f && this.carParameters.GetForwardVelocity() != 0.0f)
		{
			float direction = this.carParameters.GetForwardVelocity() > 0.0f ? -1 : 1;
			this.carBody.AddForceAtPosition(
				this.carParameters.GetBrake() * this.carParameters.GetBrakePower() * this.transform.forward * direction,
				this.transform.position
			);
		}

		if (this.powerMultiplayer > 0.0f && this.carParameters.GetThrottle() > 0.0f && Mathf.Abs(this.carParameters.GetTorque()) > 0)
		{
			float torque = this.carParameters.GetTorque() * this.carParameters.GetThrottle();
			torque *= this.powerMultiplayer;
						
			if (this.oppositeWheel != null)
			{
				float totalGrip = this.grip + this.oppositeWheel.GetCurrentGrip() + 0.001f; //So its never 0...you know...divide by 0 its a problem
				float gripRatio = this.grip / totalGrip;
				
				torque *= Mathf.Lerp(this.maxTorqueByDifferential, this.minTorqueByDifferential, gripRatio);
			}
			
			this.carBody.AddForceAtPosition(torque * this.transform.forward * this.GetPowerMultipler(), this.transform.position);
		}
		
		if(this.GetResitanceMultipler() > 0.0f && this.carParameters.GetForwardVelocity() > 5.0f){
			Vector3 wheelVelocity = this.carBody.GetPointVelocity(this.transform.position);
			Vector3 resistanceForce = -wheelVelocity.normalized * this.GetResitanceMultipler() * wheelVelocity.magnitude;
			this.carBody.AddForceAtPosition(resistanceForce, this.transform.position, ForceMode.Acceleration);
		}
	}
	
	void TurnWheel()
	{
		this.transform.localRotation = Quaternion.AngleAxis(this.carParameters.GetTurn() * this.maxTurnAngle, Vector3.up);
	}
}
