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
		this.UpdateBrake();
		this.UpdateTurn();
	}
	
	private void UpdateThrottle()
	{
		this.carParameters.SetThrottle(this.throttleInput.ReadValue<float>());
	}
	
	private void UpdateBrake()
	{
		this.carParameters.SetBrake(this.brakeInput.ReadValue<float>());
	}
	
	private void UpdateTurn()
	{
		float value = this.turnInput.ReadValue<float>();
		float responseTime = Mathf.Lerp(0.1f, 0.6f, this.carParameters.GetVelocityNormalice());
		this.currentTurnValue = Mathf.Lerp(this.currentTurnValue,  value, Time.deltaTime * 1.0f / responseTime);
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
}