using UnityEngine;

public class TerrainInfoReader : RDRSNode
{
    public enum TerrainReaderParameterType
    {
        PowerMultiplier = 0,
        GripMultiplier = 1,
        ResistanceMultiplier = 2,
        DriftGameObject = 3,
        DustGameObject = 4,
        Roughness = 7
    }

    [SerializeField] private TerrainReaderParameterType parameter;
    [SerializeField][Tooltip("Will read it using RayCast, but its better if you put a Suspension")] private SuspensionPhysic suspensionOverride;
    [SerializeField] private bool returnDefaultIfNotFound = true;
    [SerializeField] private float maxDistance = 3f;

    private LayerMask trackLayer;

    private void Awake()
    {
        this.trackLayer = 1 << LayerMask.NameToLayer("Track");
    }

    public override object GetValue()
    {
        TerrainInfo terrain = null;

        if (this.suspensionOverride != null)
        {
            terrain = this.suspensionOverride.GetCurrentTerrain();
        }
        else
        {
            Vector3 origin = this.transform.position;
            Vector3 direction = -this.transform.up;
            float distance = this.maxDistance;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, distance, this.trackLayer))
            {
                terrain = hit.collider.GetComponent<TerrainInfo>();
            }
        }

        if (terrain == null)
        {
            if (this.returnDefaultIfNotFound == false)
            {
                return null;
            }
            return this.GetDefaultValue();
        }

        return this.GetParameterFromTerrain(terrain);
    }

    private object GetParameterFromTerrain(TerrainInfo terrain)
    {
        object value = null;
        switch (this.parameter)
        {
            case TerrainReaderParameterType.PowerMultiplier:
                value = terrain.GetPowerMultipler();
                break;
            case TerrainReaderParameterType.GripMultiplier:
                value = terrain.GetGripMultipler();
                break;
            case TerrainReaderParameterType.ResistanceMultiplier:
                value = terrain.GetResistanceMultipler();
                break;
            case TerrainReaderParameterType.DriftGameObject:
                value = terrain.GetDriftGameObject();
                break;
            case TerrainReaderParameterType.DustGameObject:
                value = terrain.GetDustGameObject();
                break;
        }
        if (value is UnityEngine.Object unityObj && unityObj == null)
        {
            return null;
        }
        return value;
    }

    private object GetDefaultValue()
    {
        switch (this.parameter)
        {
            case TerrainReaderParameterType.PowerMultiplier:
            case TerrainReaderParameterType.GripMultiplier:
                return 1.0f;

            case TerrainReaderParameterType.ResistanceMultiplier:
            case TerrainReaderParameterType.Roughness:
                return 0.0f;

            case TerrainReaderParameterType.DriftGameObject:
            case TerrainReaderParameterType.DustGameObject:
                return null;

            default:
                return null;
        }
    }
}
