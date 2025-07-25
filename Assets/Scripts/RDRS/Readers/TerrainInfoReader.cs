using UnityEngine;

public class TerrainInfoReader : RDRSReaderBase
{
    public enum TerrainReaderParameterType
    {
        PowerMultiplier,
        GripMultiplier,
        ResistanceMultiplier,
        DriftParticlesGameObject,
        DustParticlesGameObject,
        DriftSoundGameObject,
        DustSoundGameObject,
        Roughness
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
            case TerrainReaderParameterType.DriftParticlesGameObject:
                value = terrain.GetDriftGameObject();
                break;
            case TerrainReaderParameterType.DustParticlesGameObject:
                value = terrain.GetDustGameObject();
                break;
            case TerrainReaderParameterType.DriftSoundGameObject:
                value = terrain.GetDriftSound();
                break;
            case TerrainReaderParameterType.DustSoundGameObject:
                value = terrain.GetDustSound();
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

            case TerrainReaderParameterType.DriftParticlesGameObject:
            case TerrainReaderParameterType.DustParticlesGameObject:
            case TerrainReaderParameterType.DriftSoundGameObject:
            case TerrainReaderParameterType.DustSoundGameObject:
                return null;

            default:
                return null;
        }
    }
}
