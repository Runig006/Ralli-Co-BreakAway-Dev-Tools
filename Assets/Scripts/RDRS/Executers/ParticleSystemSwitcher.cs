using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ParticleSystemSwitcher : RDRSReaderWithFrequency
{
    [SerializeField] private Transform parent;
    [SerializeField] private bool destroyOldImmediately = false;
    [SerializeField] private bool whenNullCleanAll = true;
    [SerializeField] private bool forceParentScale = true;

    [SerializeField] private RDRSReaderBase[] gameObjectReaders;
    private ParticleSystem[] currentSystems;

    private GameObject currentGameObject;
    private GameObject currentSource;

    public override object GetValue()
    {
        return this.currentSystems;
    }


    public override object GetExecuteValue()
    {
        object source = null;

        foreach (RDRSReaderBase reader in this.gameObjectReaders)
        {
            if (reader == null)
            {
                continue;
            }
            source = reader.GetValue();
            if (source != null)
            {
                break;
            }
        }
        return source;
    }

    public override void Execute()
    {
        object valueRaw = this.GetExecuteValue();
        if (valueRaw == null)
        {
            if (this.whenNullCleanAll)
            {
                this.cleanEverything();
            }
            return;
        }

        GameObject value = null;    
        switch (valueRaw)
        {
            case GameObject go:
                value = go;
                break;
            case ParticleSystem ps:
                value = ps.gameObject;
                break;
            default:
                Debug.LogWarning($"[ParticleSystemSwitcher] Unsupported value type: {valueRaw.GetType().Name}");
                return;
        }


        if (value == null)
        {
            Debug.LogWarning($"[ParticleSystemSwitcher] Value not valid: {valueRaw.GetType().Name}");
            return;
        }

        if (value == this.currentSource)
        {
            return;
        }

        /* Is a different System*/
        this.cleanEverything();

        this.currentSource = value;
        this.currentGameObject = Instantiate(value, (this.parent == null) ? this.gameObject.transform : this.parent.transform, false);
        this.currentGameObject.SetActive(true);
        this.currentGameObject.transform.localPosition = Vector3.zero;
        this.currentGameObject.transform.localRotation = new Quaternion(0, 0, 0, 0);
        this.currentSystems = this.currentGameObject.GetComponentsInChildren<ParticleSystem>();
        if (this.forceParentScale)
        {
            foreach (ParticleSystem sp in this.currentSystems)
            {
                var main = sp.main;
                main.scalingMode = ParticleSystemScalingMode.Hierarchy;
            }
        }
    }

    private void cleanEverything()
    {
        ParticleSystem[] systemsToDestroy = this.currentSystems;
        GameObject gameObjectToDestroy = this.currentGameObject;
        this.currentSystems = null;
        this.currentGameObject = null;
        this.currentSource = null;

        if (gameObjectToDestroy == null)
        {
            return;
        }

        if (this.destroyOldImmediately)
        {
            Destroy(gameObjectToDestroy);
        }
        else
        {
            foreach (ParticleSystem system in systemsToDestroy)
            {
                StartCoroutine(WaitForAllParticlesToEnd(systemsToDestroy, () =>
                {
                    Destroy(gameObjectToDestroy);
                }));
            }
        }
    }

    private IEnumerator WaitForAllParticlesToEnd(ParticleSystem[] systems, System.Action onComplete)
    {
        if (systems == null || systems.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        foreach (var ps in systems)
        {
            ps.Stop();
            var em = ps.emission;
            em.rateOverTime = 0.0f;
            em.rateOverDistance = 0.0f;
        }

        yield return new WaitForSeconds(0.5f);

        bool anyAlive;
        do
        {
            anyAlive = false;
            foreach (var ps in systems)
            {
                if (ps != null && ps.IsAlive(true))
                {
                    anyAlive = true;
                    break;
                }
            }

            if (anyAlive)
            {
                yield return new WaitForSeconds(0.5f);
            }

        } while (anyAlive);

        onComplete?.Invoke();
    }
}
