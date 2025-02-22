using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


public class SplineDebugView : MonoBehaviour
{
	[SerializeField] private SplineContainer splineContainer;
	[SerializeField] private float speed = 5f;
	[SerializeField] private float hoverOffset = 1.25f;
	[SerializeField] private float maxHeightDifference =  0.08f;
	[SerializeField] private float interpolationSpeed = 25f;
	[SerializeField] private LayerMask floorLayerMask;

	[Header("Debug variables")]
	public float differenceBetweenFloor; // Debug variable
	public float differenceBetweenFloorSpline;
	
	private float progress = 0.0f;
	private float previousHeight;
	
	private GameObject debugCube;
	private Vector3[] raycastPoints = new Vector3[4];
	private float maxRaycastDistance = 20f;
	private Camera debugCamera;

	void Start()
	{
		if (this.splineContainer == null)
		{
			this.splineContainer = GetComponent<SplineContainer>();
			return;
		}
		
		// Create the Debug Cube
		this.debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		this.debugCube.transform.SetParent(transform);
		this.debugCube.transform.localScale = Vector3.one;
		this.debugCube.GetComponent<Renderer>().material.color = Color.red;
		
		// Create the Debug Camera
		this.debugCamera = new GameObject("DebugCamera").AddComponent<Camera>();
		this.debugCamera.transform.SetParent(this.debugCube.transform);
		this.debugCamera.transform.localPosition = new Vector3(0, 2, -5);
		this.debugCamera.transform.LookAt(this.debugCube.transform);
		
		// Prepare
		this.previousHeight = this.transform.position.y;
		this.BuildPoints();
	}

	void Update()
	{
		if (this.splineContainer == null) {
			return;
		}
		
		this.progress += (this.speed * Time.deltaTime) / this.splineContainer.Splines[0].GetLength();
		if (this.progress > 1.0f) {
			this.progress = 0.0f;
		}
		
		this.UpdatePosition();
	}


	private void BuildPoints()
	{
		Bounds bounds = this.debugCube.GetComponent<Renderer>().bounds;
		this.raycastPoints[0] = new Vector3(bounds.extents.x, 0, bounds.extents.z); 
		this.raycastPoints[1] = new Vector3(-bounds.extents.x, 0, bounds.extents.z); 
		this.raycastPoints[2] = new Vector3(bounds.extents.x, 0, -bounds.extents.z); 
		this.raycastPoints[3] = new Vector3(-bounds.extents.x, 0, -bounds.extents.z); 
	}

	// Move it
	private void UpdatePosition()
	{
		(Vector3 position, Vector3 tangent, Vector3 groundNormal) = this.GetAllInfoFromPosition(this.splineContainer, this.progress);
		
		this.transform.rotation = Quaternion.LookRotation(tangent, groundNormal);
		this.AdjustHeightToGround(position);
	}

	private (Vector3, Vector3, Vector3) GetAllInfoFromPosition(SplineContainer container, float progress)
	{
		Vector3 position, tangent, groundNormal = Vector3.up;
		RaycastHit hit;
		float3 position3, tangent3, up3;
		
		splineContainer.Evaluate(splineContainer.Splines[0], progress, out position3, out tangent3, out up3);
		
		position = new Vector3(position3.x, position3.y, position3.z);
		tangent = new Vector3(tangent3.x, tangent3.y, tangent3.z);

		if (Physics.Raycast(position + Vector3.up * 10, Vector3.down, out hit, 20f, this.floorLayerMask))
		{
			groundNormal = hit.normal;
		}

		return (position, tangent, groundNormal);
	}

	private void AdjustHeightToGround(Vector3 position)
	{
		RaycastHit hit;
		Vector3 averageNormal = Vector3.zero;
		Vector3 originalPosition = position;
		
		float averageHeight = 0f;
		int hitCount = 0;


		//Find the points
		foreach (Vector3 localPoint in this.raycastPoints)
		{
			Vector3 worldPoint = this.debugCube.transform.TransformPoint(localPoint);
			if (Physics.Raycast(worldPoint + Vector3.up * 5f, Vector3.down, out hit, this.maxRaycastDistance, this.floorLayerMask))
			{
				Debug.DrawRay(hit.point, hit.normal * 2f, Color.blue);
				averageNormal += hit.normal;
				averageHeight += hit.point.y;
				hitCount++;
			}
		}

		if (hitCount > 0)
		{
			// Normalize the floor
			averageNormal.Normalize();
			averageHeight /= hitCount;
			
			//For Debug
			this.differenceBetweenFloor = Mathf.Abs(this.debugCube.transform.position.y - averageHeight);
			this.differenceBetweenFloorSpline = Mathf.Abs(position.y - averageHeight);
			
			float desiredHeight = averageHeight + this.hoverOffset;
			
			//If we are to low from the spliter
			if (position.y > desiredHeight + 1.0f)
			{
				desiredHeight = position.y;
			}
			//To much distance for the floor
			else if (this.differenceBetweenFloor > this.hoverOffset + this.maxHeightDifference)
			{
				desiredHeight -=  (this.differenceBetweenFloor - this.hoverOffset);
			}
			// Move it like it is a newborn
			position.y = Mathf.Lerp(this.previousHeight, desiredHeight, Time.deltaTime * this.interpolationSpeed);
			this.transform.position = position;


			///////////////////////ROLL///////////////////////////////
			// Make the roll
			Vector3 currentEuler = transform.rotation.eulerAngles;

			// Get the euler
			Quaternion targetRotation = Quaternion.FromToRotation(Vector3.up, averageNormal);
			Vector3 targetEuler = targetRotation.eulerAngles;

			// Create a custom Euler
			targetEuler = new Vector3(currentEuler.x, currentEuler.y, targetEuler.z);
			transform.rotation = Quaternion.Slerp(Quaternion.Euler(currentEuler), Quaternion.Euler(targetEuler), Time.deltaTime * 5f);
		}
		else
		{
			// If we dont find the floor, just...go to whatever the spline want
			position.y = Mathf.Lerp(this.previousHeight, position.y - this.interpolationSpeed * Time.fixedDeltaTime, Time.fixedDeltaTime * 5f);
			this.transform.position = position;
		}
		this.previousHeight = position.y;
	}
}
