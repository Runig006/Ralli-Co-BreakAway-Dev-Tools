using UnityEngine;

public class TerrainInfoReader : RDRSReaderBase
{
    public enum TerrainReaderParameterType
    {
        PowerMultiplier,
        GripMultiplier,
        ResistanceMultiplier,
        EnableDriftSound,
        DriftGameObject,
        DustGameObject,
        DustSound,
        DriftSound,
        Roughness
    }

    [SerializeField] private TerrainReaderParameterType parameter;
    [SerializeField] [Tooltip("Will read it using RayCast, but its better if you put a Suspension")] private SuspensionPhysic suspensionOverride;
    [SerializeField] private bool returnDefaultIfNotFound = true;
    [SerializeField] private float maxDistance = 3f;
    
    private int trackLayer;
    
    private void Awake()
    {
        this.trackLayer = LayerMask.NameToLayer("Track");
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
            if (Physics.Raycast(this.transform.position, Vector3.down, out RaycastHit hit, this.maxDistance, this.trackLayer))
            {
                terrain = hit.collider.GetComponent<TerrainInfo>();
            }
        }
        if (terrain == null)
        {
            if (!returnDefaultIfNotFound)
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
            case TerrainReaderParameterType.EnableDriftSound:
                value = terrain.GetEnableDriftSound();
                break;
            case TerrainReaderParameterType.DriftGameObject:
                value = terrain.GetDriftGameObject();
                break;
            case TerrainReaderParameterType.DustGameObject:
                value = terrain.GetDustGameObject();
                break;
            case TerrainReaderParameterType.DustSound:
                value = terrain.GetDustSound();
                break;
            case TerrainReaderParameterType.DriftSound:
                value = terrain.GetDriftSound();
                break;
            case TerrainReaderParameterType.Roughness:
                value = terrain.GetRoughness();
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

            case TerrainReaderParameterType.EnableDriftSound:
                return true;

            case TerrainReaderParameterType.DriftGameObject:
            case TerrainReaderParameterType.DustGameObject:
            case TerrainReaderParameterType.DustSound:
            case TerrainReaderParameterType.DriftSound:
                return null;

            default:
                return null;
        }
    }
}
