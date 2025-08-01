using UnityEngine;


public class FixedValueReader : RDRSNode
{
    public enum FixedReaderValueType
    {
        Float = 0,
        String = 1,
        Vector2 = 2,
        Vector3 = 3,
        Object = 4,
        ObjectArray = 5
    }



    [SerializeField] private FixedReaderValueType valueType = FixedReaderValueType.Float;

    [SerializeField] private float floatValue = 0f;
    [SerializeField] private string stringValue = "";
    [SerializeField] private Vector2 vector2Value;
    [SerializeField] private Vector3 vector3Value;
    [SerializeField] private Object objectValue;
    [SerializeField] private Object[] objectsValue;

    public override object GetValue()
    {
        switch (this.valueType)
        {
            case FixedReaderValueType.Float:
                return this.floatValue;
            case FixedReaderValueType.String:
                return this.stringValue;
            case FixedReaderValueType.Vector2:
                return this.vector2Value;
            case FixedReaderValueType.Vector3:
                return this.vector3Value;
            case FixedReaderValueType.Object:
                return this.objectValue != null ? this.objectValue : null;
            case FixedReaderValueType.ObjectArray:
                return this.objectsValue != null ? this.objectsValue : new Object[0];
            default:
                return null;
        }
    }
}
