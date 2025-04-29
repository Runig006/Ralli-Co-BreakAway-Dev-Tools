using UnityEngine;

public class SpliterTester : MonoBehaviour
{
    [SerializeField] private GameObject spliter;
    [SerializeField] private Transform spliterPoint;
    [SerializeField] private Transform trackPoint;

    void Update()
    {
        if (this.spliter == null || this.spliterPoint == null || this.trackPoint == null)
        {
            return;
        }

        this.spliter.transform.position = Vector3.zero;
        this.spliter.transform.rotation = Quaternion.identity;
        
        Vector3 endPointPosition = this.trackPoint.position;
        Vector3 endPointForward = this.trackPoint.forward;
        Vector3 startPointForward = this.spliterPoint.forward;
        Quaternion alignmentRotation = BuildRotation(startPointForward, -endPointForward);
        
        this.spliter.transform.rotation = alignmentRotation;
		Vector3 positionOffset = endPointPosition - this.spliterPoint.position;
		this.spliter.transform.position += positionOffset;
    }

	private Quaternion BuildRotation(Vector3 startPoint, Vector3 endPoint)
	{
		Quaternion alignmentRotation = Quaternion.FromToRotation(startPoint, endPoint);
		Vector3 alignmentEuler = alignmentRotation.eulerAngles;
		if (Mathf.Abs(alignmentEuler.z) > 90f)
		{
			alignmentEuler.z = 0f;
			alignmentRotation = Quaternion.Euler(alignmentEuler);
		}
		return alignmentRotation;
	}
}
