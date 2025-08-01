using UnityEngine;
using UnityEngine.InputSystem;

public class UserController : MonoBehaviour
{	
	private CarParameters carParameters;
	private PlayerInput input;
	private CameraManager cameraManager;
	
	private InputAction throttleInput;
	private InputAction brakeInput;
	private InputAction turnInput;
	private InputAction boostInput;
	
	private float currentTurnValue = 0.0f;
	private float turnVelocity = 0.0f;
	private float automaticClutchTimer = 0.0f;

	public void Awake()
	{
		this.carParameters = GetComponentInParent<CarParameters>();
		this.input = GetComponent<PlayerInput>();
		this.cameraManager = transform.parent.gameObject.GetComponentInChildren<CameraManager>();
		
		this.throttleInput = this.input.actions["throttle"];
		this.brakeInput = this.input.actions["brake"];
		this.turnInput = this.input.actions["turn"];
		this.boostInput = this.input.actions["boost"];
	}
	
	
	// Update is called once per frame
	public void Update()
	{
		this.carParameters.SetBoosting(this.boostInput.IsPressed());
		this.UpdateThrottle();
		this.UpdateClutch();
		this.UpdateBrake();
		this.UpdateTurn();
		
		if(this.carParameters.GetCurrentGear() == null)
		{
			this.ChangeGear(0);
		}
	}
	
	private void UpdateThrottle()
	{
		this.carParameters.SetThrottle(this.throttleInput.ReadValue<float>());
	}
	
	private void UpdateClutch()
	{
		float clutchValue;
		if (this.automaticClutchTimer > 0)
		{
			this.automaticClutchTimer -= Time.deltaTime;
			clutchValue = 1.0f;
		}
		else
		{
			clutchValue = 0.0f;
		}
		this.carParameters.SetClutch(clutchValue);
	}
	
	private void UpdateBrake()
	{
		this.carParameters.SetBrake(this.brakeInput.ReadValue<float>());
	}
	
	private void UpdateTurn()
	{
		float value = this.turnInput.ReadValue<float>();
		float responseTime = Mathf.Lerp(0.15f, 0.5f, this.carParameters.GetVelocityNormalize());
		this.currentTurnValue =  Mathf.SmoothDamp(
			currentTurnValue,
			value,
			ref this.turnVelocity,
			responseTime,
			Mathf.Infinity,
			Time.deltaTime
		);
		this.carParameters.SetTurn(this.currentTurnValue);
	}
	
	public void OnShiftUp(InputValue value)
	{
		this.carParameters.SetGear(this.carParameters.GetCurrentGear() + 1);
	}
	
	public void OnShiftDown(InputValue value)
	{
		this.carParameters.SetGear(this.carParameters.GetCurrentGear() - 1);
	}
	
	public void OnCamera(InputValue inputValue)
	{
		this.cameraManager.NextCamera();
	}
	
	private void ChangeGear(int? value)
	{
		if (value != null && (value > this.carParameters.GetMaxGear() || value < -1))
		{
			return;
		}

		this.automaticClutchTimer = this.carParameters.GetClutchDelay();
		this.carParameters.SetClutch(1.0f);
		this.carParameters.SetGear(value);
	}

}