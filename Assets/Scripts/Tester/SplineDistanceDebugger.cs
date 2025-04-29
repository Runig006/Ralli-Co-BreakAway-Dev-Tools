using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineDistanceDebugger : MonoBehaviour
{
	[SerializeField] private SplineContainer splineContainer;
	[SerializeField] private MeshRenderer roadMeshRenderer;
	[SerializeField] private float maxAllowedDistance = 0.5f;

	[ContextMenu("Debug Spline Points Distance")]
	public void DebugSplineDistances()
	{
		if (splineContainer == null || roadMeshRenderer == null)
		{
			Debug.LogWarning("SplineContainer or RoadMeshRenderer is not assigned.");
			return;
		}
		CheckSplineDistances(splineContainer);
	}

	private void CheckSplineDistances(SplineContainer splineContainer)
	{
		Spline spline = splineContainer.Splines[0];
		for (int i = 0; i < spline.Count; i++)
		{
			Vector3 splinePoint = spline[i].Position;
			if (Physics.Raycast(splinePoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
			{
				float distance = Vector3.Distance(splinePoint, hit.point);
				if (distance > maxAllowedDistance)
				{
					Debug.DrawRay(splinePoint, Vector3.down * distance, Color.red, 5f);
					Debug.Log($"Point {i} is too far from the ground: {distance} units.");
				}
			}
		}
	}
}
