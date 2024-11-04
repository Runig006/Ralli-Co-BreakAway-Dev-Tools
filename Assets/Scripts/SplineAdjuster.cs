using UnityEngine;
using UnityEngine.Splines;

[ExecuteInEditMode]
public class SplineAdjuster : MonoBehaviour
{
	[SerializeField] private SplineContainer splineContainer;
	[SerializeField] private MeshRenderer roadMeshRenderer;
	[SerializeField] private float heightAboveGround = 0.2f;

	[ContextMenu("Adjust Spline to Road Mesh")]
	public void AdjustSpline()
	{
		if (splineContainer == null || roadMeshRenderer == null)
		{
			Debug.LogWarning("SplineContainer or RoadMeshRenderer is not assigned.");
			return;
		}
		this.AdjustSplineToMesh(splineContainer);
	}

	private void AdjustSplineToMesh(SplineContainer splineContainer)
	{
		Spline spline = splineContainer.Splines[0];
		for (int i = 0; i < spline.Count; i++)
		{
			Vector3 splinePoint = spline[i].Position;
			if (Physics.Raycast(splinePoint + Vector3.up * 10f, Vector3.down, out RaycastHit hit, 20f))
			{
				splinePoint = hit.point + Vector3.up * heightAboveGround;
			}
			spline[i] = new BezierKnot(splinePoint, spline[i].TangentIn, spline[i].TangentOut, spline[i].Rotation);
		}
	}
}
