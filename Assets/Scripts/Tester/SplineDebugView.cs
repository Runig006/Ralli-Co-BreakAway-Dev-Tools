using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;


public class SplineDebugView : MonoBehaviour
{
	[SerializeField] private SplineContainer splineContainer;
	[SerializeField] private int splineIndex = 0;
	[SerializeField] private float speed = 5f;
	[SerializeField] private float currentLane = 0;
	[SerializeField] private bool evenLaneNumber = false;

	[Header("Debug variables")]
	public float differenceBetweenFloor; // Debug variable
	public float differenceBetweenFloorSpline;
	
	private float progress = 0.0f;
	private float previousHeight;
	
	private GameObject debugCube;
	private Camera debugCamera;

	private LayerMask floorLayerMask;

	private Vector3[] raycastPoints = new Vector3[4];
	private float hoverOffset = 1.25f;
	private float interpolationSpeed = 20f;
	
	private float rayCastAddedHeight = 5.0f;
	private float maxRaycastDistance = 10f;
	
	private SplineContainer initialSplineContainer;
    private int initialSplineIndex;

	void Start()
	{
		if (this.splineContainer == null)
		{
			this.splineContainer = GetComponent<SplineContainer>();
			return;
		}

		this.floorLayerMask = LayerMask.GetMask("Track");
		
		// Create the Debug Cube
		this.debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
		this.debugCube.transform.SetParent(transform);
		this.debugCube.transform.localScale = new Vector3(1.5f,1.2f,3.7f);
		this.debugCube.GetComponent<Renderer>().material.color = Color.red;
		
		// Create the Debug Camera
		this.debugCamera = new GameObject("DebugCamera").AddComponent<Camera>();
		this.debugCamera.transform.SetParent(this.transform);
		this.debugCamera.transform.localPosition = new Vector3(0, 2.5f, -6);
		

		this.debugCamera.transform.rotation = Quaternion.Euler(new Vector3(14f,0f,0f));
		
		// Prepare
		this.previousHeight = this.transform.position.y;
		this.BuildPoints();
		
		this.initialSplineContainer = this.splineContainer;
		this.initialSplineIndex = this.splineIndex;
	}

	void Update()
	{
		if (this.splineContainer == null) {
			return;
		}
		
		this.progress += (this.speed * Time.deltaTime) / this.splineContainer.Splines[0].GetLength();
		if (this.progress > 1.0f) {
			this.splineContainer = this.FindNearbySplineContainer();
			this.splineIndex = Random.Range(0,this.splineContainer.Splines.Count);
			this.progress = 0.0f;
		}
		
		this.UpdatePosition();
	}
	
	private SplineContainer FindNearbySplineContainer()
    {
        SplineContainer[] allSplines = FindObjectsByType<SplineContainer>(FindObjectsSortMode.None);
        float closestDistance = 20.0f;
        SplineContainer closestSpline = null;

        foreach (SplineContainer spline in allSplines)
        {
            if (spline == this.splineContainer) continue;
            
            Vector3 startPoint = spline.transform.TransformPoint(spline.Splines[0][0].Position);
            float distance = Vector3.Distance(this.transform.position, startPoint);
            
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSpline = spline;
            }
        }
        
        return closestSpline ?? this.initialSplineContainer;
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
		float offsetByLane = this.GetTotalOffsetByLane();
		(Vector3 position, Vector3 tangent, Vector3 groundNormal) = this.GetAllInfoFromPosition(this.splineContainer, this.progress);
		position += Vector3.Cross(groundNormal, tangent).normalized * offsetByLane;		
		
		Quaternion targetRotation = Quaternion.LookRotation(tangent, groundNormal);
		targetRotation = Quaternion.Slerp(this.gameObject.transform.rotation, targetRotation, Time.deltaTime * 10.0f);
		
		Vector3 eulerRotation = targetRotation.eulerAngles;
		eulerRotation.z = this.transform.rotation.eulerAngles.z;
		this.transform.rotation = Quaternion.Euler(eulerRotation);

		this.AdjustHeightToGround(position);
	}
	
	private float GetTotalOffsetByLane()
	{
		float total = Mathf.Abs(this.currentLane) * 4.3f;
		if(this.evenLaneNumber == true)
		{
		    total-=2.15f;
		}
		return total * Mathf.Sign(this.currentLane);
	}

	private (Vector3, Vector3, Vector3) GetAllInfoFromPosition(SplineContainer container, float progress)
	{
		Vector3 position, tangent, groundNormal = Vector3.up;
		RaycastHit hit;
		float3 position3, tangent3, up3;
		
		splineContainer.Evaluate(splineContainer.Splines[this.splineIndex], progress, out position3, out tangent3, out up3);
		
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
		
		float averageHeight = 0f;
		int hitCount = 0;
		
		//Find the points
		foreach (Vector3 localPoint in this.raycastPoints)
		{
			Vector3 worldPoint = this.transform.TransformPoint(localPoint);
			if (Physics.Raycast(worldPoint + Vector3.up * this.rayCastAddedHeight, Vector3.down, out hit, this.maxRaycastDistance, this.floorLayerMask))
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
			Debug.DrawRay(transform.position, averageNormal * 2f, Color.green);

			averageHeight /= hitCount;
			
			float desiredHeight = averageHeight + this.hoverOffset;	
					
			//If we are too low from the spline
			if (desiredHeight < position.y - 1.0f)
			{
				desiredHeight = position.y;
			}
			//We are too high for the spline
			else if	(desiredHeight > position.y + 5.0f)
			{
				desiredHeight = position.y;
			}
			
			// Move it like it is a newborn
			position.y = Mathf.Lerp(this.previousHeight, desiredHeight, Time.deltaTime * this.interpolationSpeed);
			this.transform.position = position;


			///////////////////////ROLL///////////////////////////////
			Quaternion targetRotation = Quaternion.LookRotation(transform.forward, averageNormal);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * this.interpolationSpeed);

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
