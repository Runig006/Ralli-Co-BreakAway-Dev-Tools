using UnityEngine;

public abstract class RDRSReaderBase : MonoBehaviour, IRDRSReader
{
    public string Tag;
    public abstract object GetValue();
}
