using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectSwitcher : RDRSNodeWithFrequency
{
    [SerializeField] private Transform parent;
    [SerializeField] private bool whenNullCleanAll = true;

    [SerializeField] private RDRSNode[] gameObjectReaders;
    
    [SerializeField] private float smoothMaxTime = 1.00f;

    private GameObject currentInstance;
    private GameObject currentSource;

    public List<GameObject> currentInstances = new List<GameObject>();
    
    public override object GetValue()
    {
        return this.currentInstances.ToArray(); //List Doesnt work well with ReferenceEquals
    }

    #region Events
    protected override void OnDisable()
    {
        base.OnDisable();
        GameObject toDestroy = this.currentInstance;
        this.currentInstance = null;
        this.currentSource = null;
        this.currentInstances.Clear();
        
        if (toDestroy != null)
        {
            Destroy(toDestroy);
        }
    }

    private void OnDestroy()
    {
        GameObject toDestroy = this.currentInstance;
        this.currentInstance = null;
        this.currentSource = null;
        this.currentInstances.Clear();
        
        if (toDestroy != null)
        {
            Destroy(toDestroy);
        }
    }
    #endregion

    public override void Execute()
    {
        object valueRaw = this.GetRawValue();
        if (valueRaw == null)
        {
            if (this.whenNullCleanAll)
            {
                this.CleanEverything();
            }
            return;
        }

        GameObject value = GetGameObject(valueRaw);
        if (value == null)
        {
            Debug.LogWarning($"[GameObjectSwitcher] Unsupported value type: {valueRaw.GetType().Name}");
            return;
        }
        
        if (ReferenceEquals(value, this.currentSource))
        {
            return;
        }

        this.CleanEverything();

        this.currentSource = value;
        
        this.currentInstance = Instantiate(value, (this.parent == null) ? this.transform : this.parent, false);
        this.currentInstance.SetActive(true);
        this.currentInstances.Add(this.currentInstance);
        
        this.currentInstance.transform.localPosition = Vector3.zero;
        this.currentInstance.transform.localRotation = Quaternion.identity;
    }
    
    #region Get Object To Switch
    public object GetRawValue()
    {
        object source = null;

        if (this.gameObjectReaders == null || this.gameObjectReaders.Length == 0)
        {
            return null;
        }
        foreach (RDRSNode reader in this.gameObjectReaders)
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
    
    private GameObject GetGameObject(object valueRaw)
    {
        GameObject value = valueRaw as GameObject;
        if (value == null)
        {
            Component comp = valueRaw as Component;
            if (comp != null)
            {
                value = comp.gameObject;
            }
            else
            {
                Transform tr = valueRaw as Transform;
                if (tr != null)
                {
                    value = tr.gameObject;
                }
            }
        }
        return value;
    }
    #endregion

    #region Clean Logic
    private void CleanEverything()
    {
        GameObject toDestroy = this.currentInstance;
        this.currentInstance = null;
        this.currentSource = null;

        if (toDestroy == null)
        {
            return;
        }

        if (this.smoothMaxTime > 0.0f)
        {
            this.StartCoroutine(this.smoothReleaseRoutine(toDestroy));
            return;
        }
        else
        {
            this.currentInstances.Remove(toDestroy);
            Destroy(toDestroy);
        }
    }

    #region Coroutine
    private IEnumerator smoothReleaseRoutine(GameObject target)
    {
        if (target == null)
        {
            yield break;
        }
        ParticleSystem[] psAll = target.GetComponentsInChildren<ParticleSystem>(true);
        foreach (ParticleSystem ps in psAll)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }
        
        AudioSource[] audioAll = target.GetComponentsInChildren<AudioSource>(true);
        foreach (AudioSource a in audioAll)
        {
            a.loop = false;
        }

        float deadline = Time.time + this.smoothMaxTime;
        while (Time.time < deadline)
        {
            if (this.AnyAlive(psAll, audioAll))
            {
                yield return null;
                continue;
            }

            break;
        }
        this.currentInstances.Remove(target);
        Object.Destroy(target);
    }
    
    private bool AnyAlive(ParticleSystem[] psAll, AudioSource[] audioAll)
    {
        foreach (ParticleSystem ps in psAll)
        {
            if (ps.IsAlive(true))
            {
                return true;
            }
        }

        foreach (AudioSource a in audioAll)
        {
            if (a.isPlaying)
            {
                return true;
            }
        }

        return false;
    }
    #endregion
    #endregion
}
