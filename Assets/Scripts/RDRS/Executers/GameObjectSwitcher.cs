using System.Collections;
using UnityEngine;

public class GameObjectSwitcher : RDRSNodeWithFrequency
{
    [SerializeField] private Transform parent;
    [SerializeField] private bool whenNullCleanAll = true;

    [SerializeField] private RDRSNode[] gameObjectReaders;

    private GameObject currentInstance;
    private GameObject currentSource;

    public override object GetValue()
    {
        return this.currentInstance;
    }

    public object GetRawValue()
    {
        object source = null;

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

    public override void Execute()
    {
        object valueRaw = this.GetRawValue();
        if (valueRaw == null)
        {
            if (this.whenNullCleanAll)
            {
                this.cleanEverything();
            }
            return;
        }

        GameObject value = valueRaw as GameObject;
        if (value == null)
        {
            Debug.LogWarning($"[GameObjectSwitcher] Unsupported value type: {valueRaw.GetType().Name}");
            return;
        }

        if (value == this.currentSource)
        {
            return;
        }

        this.cleanEverything();

        this.currentSource = value;
        this.currentInstance = Instantiate(value, (this.parent == null) ? this.transform : this.parent, false);
        this.currentInstance.SetActive(true);
        this.currentInstance.transform.localPosition = Vector3.zero;
        this.currentInstance.transform.localRotation = Quaternion.identity;
    }

    private void cleanEverything()
    {
        GameObject toDestroy = this.currentInstance;
        this.currentInstance = null;
        this.currentSource = null;

        if (toDestroy == null)
        {
            return;
        }

        Destroy(toDestroy);
    }
}
