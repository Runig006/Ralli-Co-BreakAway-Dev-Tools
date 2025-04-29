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
	[SerializeField] private float wheelWeight = 5.0f;
	
	[Header("Suspension Settings Height")]
	[SerializeField] private float springRestPosition = 0.15f;
	[SerializeField] private float springMaxTravel = 0.25f;
	[SerializeField] private float wheelRadius = 0.4f;
	
	[Header("Suspension Settings (Only Visual)")]
	[SerializeField] private float wheelMinY = 0.00f;
	[SerializeField] private float wheelMaxY = 0.00f;
	[SerializeField] private bool useThisForDriftAudio = false;
	[SerializeField] private Transform wheel = null;
	
	[Header("Drift")]
	[Range(0,1)][SerializeField] private float driftMinSideForce = 0.5f;
	
	//Others
	private CarParameters carParameters;
	private Rigidbody carBody;
	
	private float grip;
	

	private float currentWheelRotation = 0;
	
	private bool isGrounded = true;
	private bool terrainFound = false;
	private RaycastHit hitFloor;
	
	private float sideForce;
	private int sideForceDirection;
	
	private float suspensionForce;
	private float suspensionForceNormalice;
	
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

	/* Is normaliced between 0 and 1 */
	public float GetSideForce()
	{
		return this.sideForce;
	}
	
	public float GetGrip()
	{
		return this.grip;
	}
	
	public int GetSideForceDirection()
	{
		return this.sideForceDirection;
	}
	
	public float GetSuspensionForce()
	{
		return this.suspensionForce; 
	}

	public float GetSuspensionForceNormalice()
	{
		return this.suspensionForceNormalice;
	}
	
	public bool GetUseThisForDriftAudio()
	{
		return this.useThisForDriftAudio;
	}
	
		
	public bool GetDriftEnable()
	{
		return this.driftEnable;
	}
	
	public bool GetDustEnable()
	{
		return this.dustEnable;
	}
	
	public float GetSpringRestPosition()
	{
		return this.springRestPosition;
	}
	
	public float GetSpringMaxTravel()
	{
		return this.springMaxTravel;
	}
	
	public float GetWheelRadius()
	{
		return this.wheelRadius;
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
		this.RotateWheelSpeed();
		if(this.turnWheel)
		{
			this.TurnWheel();
		}
		
		this.CalculateSuspension();
		if(this.isGrounded)
		{
			this.MoveVerticalWheel();
			this.CalculateRotation();
			this.CalculateSpeed();
		}
		this.CheckForDust();
		this.CheckForDrift();
	}
	
	void CalculateSuspension()
	{
		float maxLength = this.springRestPosition + this.springMaxTravel;
		this.isGrounded = Physics.Raycast(this.transform.position, this.transform.up * -1, out this.hitFloor, maxLength + this.wheelRadius, this.carParameters.GetMaskDrivable());
		if(this.isGrounded)
		{
			float currentSpringLength = this.hitFloor.distance - this.wheelRadius;
			float springCompression = (this.springRestPosition - currentSpringLength) / this.springMaxTravel;
			float springVelocity = Vector3.Dot(this.carBody.GetPointVelocity(this.transform.position), this.transform.up);
			float netForce = (springCompression * this.springStiff) - (springVelocity * this.damping);
			
			this.suspensionForce = netForce;
			this.suspensionForceNormalice = Mathf.Clamp(suspensionForce / this.springStiff * this.springMaxTravel, -1f, 1f);

			this.carBody.AddForceAtPosition(netForce * this.transform.up, this.transform.position);
		}
		
		this.terrainFound = Physics.Raycast(this.transform.position, this.transform.up * -1, out this.hitFloor, 10.0f, this.carParameters.GetMaskDrivable());
		if(this.terrainFound)
		{
			this.currentTerrain = this.hitFloor.collider.GetComponent<TerrainInfo>();
		}else
		{
			this.currentTerrain = null;
		}
	}
	
	void CalculateRotation()
	{
		if(this.isGrounded)
		{
			//Get the force of the wheels going sideway and normalice between 0-1
			Vector3 steeringDir = this.transform.right;
			Vector3 tireWorldVel = this.carBody.GetPointVelocity(this.transform.position);
			float steeringVel = Vector3.Dot(steeringDir, tireWorldVel);
			float normalized = Mathf.Clamp01(Mathf.Abs(steeringVel) / this.maxLateralSpeed);
			
			//Set the variables for external scripts
			this.sideForce = normalized;
			this.sideForceDirection = steeringVel >= 0 ? -1 : 1;
			
			//The real logic
			float newGrip = this.gripCurve.Evaluate(normalized);
			this.grip = Mathf.Lerp(this.grip, newGrip, 0.5f);
			
			float desiredVelChange = -steeringVel * this.grip;
			float desirecAccel = desiredVelChange / Time.fixedDeltaTime;
			this.carBody.AddForceAtPosition(steeringDir * this.wheelWeight * desirecAccel * this.GetGripMultiplier(), this.transform.position);
		}
	}
	
	private void CalculateSpeed()
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
				float oppositeGrip = this.oppositeWheel.grip;
				float totalGrip = this.grip + this.oppositeWheel.GetGrip() + 0.001f;
				float gripRatio = this.grip / totalGrip;
				
				torque *= Mathf.Lerp(this.maxTorqueByDifferential, this.minTorqueByDifferential, gripRatio);
			}
			
			this.carBody.AddForceAtPosition(this.grip * torque * this.transform.forward * this.GetPowerMultipler(), this.transform.position);
		}
		
		if(this.GetResitanceMultipler() > 0.0f && this.carParameters.GetForwardVelocity() > 5.0f){
			Vector3 wheelVelocity = this.carBody.GetPointVelocity(this.transform.position);
			Vector3 resistanceForce = -wheelVelocity.normalized * this.GetResitanceMultipler() * wheelVelocity.magnitude;
			this.carBody.AddForceAtPosition(resistanceForce, this.transform.position, ForceMode.Acceleration);
		}
	}
	
	//Terrain
	private void CheckForDust()
	{
		if(this.isGrounded && this.carParameters.GetForwardVelocity() > 8.0f)
		{
			this.dustEnable = true;
		}
		
		else
		{
			this.dustEnable = false;
		}
	}
	
	private void CheckForDrift()
	{
		if(this.sideForce > this.driftMinSideForce && this.isGrounded)
		{
			this.driftEnable = true;
		}
		
		else
		{
			this.driftEnable = false;
		}
	}
	
	
	// Animations
	void MoveVerticalWheel()
	{
		if(this.wheel == null)
		{
		    return;
		}
		Vector3 newPosition = this.wheel.localPosition;
		newPosition.y = (this.hitFloor.distance - this.wheelRadius) * -1;
		if(newPosition.y <= this.wheelMinY)
		{
			newPosition.y = this.wheelMinY;
		}
		if(newPosition.y >= this.wheelMaxY)
		{
			newPosition.y = this.wheelMaxY;
		}
		this.wheel.localPosition = newPosition;
	}
	
	void RotateWheelSpeed()
	{
		if(this.wheel == null)
		{
		    return;
		}
		float wheelCircumference = 2 * Mathf.PI * this.wheelRadius;
		float rotationIncrement = (this.carParameters.GetForwardVelocity() / wheelCircumference) * 360 * Time.deltaTime;
		this.currentWheelRotation += rotationIncrement;
		
		if(this.currentWheelRotation >= 360)
		{
			this.currentWheelRotation -= 360;
		}
		if(this.currentWheelRotation <= -360)
		{
			this.currentWheelRotation = 360;
		}
		this.wheel.localRotation = Quaternion.Euler(this.currentWheelRotation, this.transform.localEulerAngles.y, 0);
	}
	
	void TurnWheel()
	{
		this.transform.localRotation = Quaternion.AngleAxis(this.carParameters.GetTurn() * this.maxTurnAngle, Vector3.up);
	}
}
