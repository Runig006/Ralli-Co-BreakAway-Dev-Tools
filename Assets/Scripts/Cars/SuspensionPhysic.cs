using System;
using UnityEngine;

public class SuspensionPhysic : MonoBehaviour
{	
	[Header("Engine Settings")]
	[SerializeField] private bool speedWheel = false;
	[SerializeField] private bool rotateWheel = true;
	[SerializeField] private bool backGrip = false;
	[SerializeField] private float differentialMultiplier = 0.0f; //If the other wheel is in the air, use this Multiplier
	[SerializeField] private SuspensionPhysic differentialWheel;
	
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
	
	//Others
	private CarParameters carParameters;
	private Rigidbody carBody;
	
	private float grip;
	
	private Transform wheel = null;
	private float currentWheelRotation = 0;
	
	private bool isGrounded = true;
	private bool terrainFound = false;
	private RaycastHit hitFloor;
	
	private float sideForce;
	private int sideForceDirection;
	
	private float suspensionForce;
	private float suspensionForceNormalice;
	
	private TerrainInfo currentTerrain;
		
	//GETTERS/SETTER
	public bool GetGrounded()
	{
		return this.isGrounded;
	}
	
	public bool GetRotateWheel()
	{
		return this.rotateWheel;
	}

	/* Is normaliced between 0 and 1 */
	public float GetSideForce()
	{
		return this.sideForce;
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
	
	//Terrain
	public TerrainInfo GetCurrentTerrain()
	{
		return this.currentTerrain;
	}
	
	public float GetPowerMultipler()
	{
		if(this.currentTerrain)
		{
			return Mathf.Lerp(1, this.currentTerrain.GetPowerMultipler(), this.carParameters.GetPowerStep());
		}else{
			return 1;
		}
	}
	
	public float GetGripMultiplier()
	{
		if(this.currentTerrain)
		{
			float grip = this.backGrip ? this.currentTerrain.GetBackGripMultipler() : this.currentTerrain.GetGripMultipler();
			float step = this.backGrip ? this.carParameters.GetBackGripStep() : this.carParameters.GetGripStep();
			return Mathf.Lerp(1, grip, step);
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
		this.wheel = this.carParameters.transform.Find("Wheels").Find(this.name);
	}
	
	void FixedUpdate()
	{		
		this.RotateWheelSpeed();
		if(this.rotateWheel)
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
			float normalized = Mathf.Clamp01(Mathf.Abs(steeringVel) / this.carParameters.GetMaxLateralSpeed());
			
			//Set the variables for external scripts
			this.sideForce = normalized;
			this.sideForceDirection = steeringVel >= 0 ? -1 : 1;
			
			//The real logic
			float newGrip = this.carParameters.GetGripCurve(this.backGrip).Evaluate(normalized);
			this.grip = Mathf.Lerp(this.grip, newGrip, 0.5f);
			
			float desiredVelChange = -steeringVel * this.grip;
			float desirecAccel = desiredVelChange / Time.fixedDeltaTime;
			this.carBody.AddForceAtPosition(steeringDir * this.wheelWeight * desirecAccel * this.GetGripMultiplier(), this.transform.position);
		}
	}
	
	void CalculateSpeed()
	{
		//If the car is overRpm or no throttle is not pressing drag
		if(this.carParameters.GetForwardVelocity() > 1.00f && (this.carParameters.GetThrottle() == 0.0f || this.carParameters.GetRPMNormalice(true) > 1.0f))
		{
			this.carBody.AddForceAtPosition(this.carParameters.GetEngineBrake() * this.transform.forward * -1, this.transform.position);
		}
		
		//If is braking..brake
		if(this.carParameters.GetBrake() > 0.0f && this.carParameters.GetForwardVelocity() != 0.00f){
			float direction = this.carParameters.GetForwardVelocity() > 0.0f ? -1 : 1;
			this.carBody.AddForceAtPosition(this.carParameters.GetBrake() * this.carParameters.GetBrakePower() * this.transform.forward * direction, this.transform.position);
		}
		
		if(this.speedWheel && this.carParameters.GetThrottle() > 0.0f){
			float torque = this.carParameters.GetTorque() * this.carParameters.GetThrottle();
			if(this.differentialWheel != null && this.differentialWheel.GetGrounded() == false)
			{
				torque *= this.differentialMultiplier;
			}
			this.carBody.AddForceAtPosition(this.grip * torque * this.transform.forward * this.GetPowerMultipler(), this.transform.position);
		}
	}
	
	// Animations
	void MoveVerticalWheel()
	{
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
		this.transform.localRotation = Quaternion.AngleAxis(this.carParameters.GetTurn() * this.carParameters.GetMaxTurnAngle(), Vector3.up);
	}
}
