using UnityEngine;

public class VisualConnector : MonoBehaviour
{
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private float originalLength = 1f;
    [SerializeField] private Vector3 pivotOffsetLocal;
    
    private Vector3 baseScale;

    void Start()
    {
        this.baseScale = transform.localScale;
    }
    
    void LateUpdate()
    {
        Vector3 dir = this.pointA.position - this.pointB.position;
        float currentLength = dir.magnitude;
        this.targetTransform.position = this.pointA.position;
        this.targetTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        
        Vector3 rotatedOffset = this.targetTransform.rotation * pivotOffsetLocal;
        this.targetTransform.position = pointA.position + rotatedOffset;
        
        float scaleFactor = Mathf.Max(0.0001f, currentLength / originalLength);
        this.targetTransform.localScale = new Vector3(
            baseScale.x,
            baseScale.y * scaleFactor,
            baseScale.z
        );
    }
}
