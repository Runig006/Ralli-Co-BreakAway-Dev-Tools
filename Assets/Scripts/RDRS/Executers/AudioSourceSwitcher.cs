using System.Collections;
using UnityEngine;

public class AudioSourceSwitcher : RDRSReaderWithFrequency
{
    [SerializeField] private Transform parent;
    [SerializeField] private bool destroyOldImmediately = false;
    [SerializeField] private bool whenNullCleanAll = true;
    [SerializeField] private float fadeOutSpeed = 0.1f;

    [SerializeField] private RDRSReaderBase[] gameObjectReaders;
    private AudioSource[] currentSources;

    private GameObject currentGameObject;
    private GameObject currentSource;

    public override object GetValue()
    {
        return this.currentSources;
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
            case AudioSource audio:
                value = audio.gameObject;
                break;
            default:
                Debug.LogWarning($"[AudioSourceSwitcher] Unsupported value type: {valueRaw.GetType().Name}");
                return;
        }

        if (value == this.currentSource)
        {
            return;
        }

        this.cleanEverything();

        this.currentSource = value;
        this.currentGameObject = Instantiate(value, (this.parent == null) ? this.transform : this.parent, false);
        this.currentGameObject.SetActive(true);
        this.currentGameObject.transform.localPosition = Vector3.zero;
        this.currentGameObject.transform.localRotation = Quaternion.identity;

        this.currentSources = this.currentGameObject.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in this.currentSources)
        {
            source.Play();
        }
    }

    private void cleanEverything()
    {
        AudioSource[] sourcesToDestroy = this.currentSources;
        GameObject gameObjectToDestroy = this.currentGameObject;
        this.currentSources = null;
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
            StartCoroutine(WaitForAllAudioToEnd(sourcesToDestroy, () =>
            {
                Destroy(gameObjectToDestroy);
            }));
        }
    }

    private IEnumerator WaitForAllAudioToEnd(AudioSource[] sources, System.Action onComplete)
    {
        if (sources == null || sources.Length == 0)
        {
            onComplete?.Invoke();
            yield break;
        }

        foreach (AudioSource src in sources)
        {
            if (src != null)
            {
                src.loop = false;
            }
        }

        bool fading = this.fadeOutSpeed > 0f;
        bool stillActive;

        do
        {
            stillActive = false;

            foreach (AudioSource src in sources)
            {
                if (src == null)
                {
                    continue;
                }
                if (fading)
                {
                    if (src.volume > 0f)
                    {
                        src.volume = Mathf.Max(0f, src.volume - this.fadeOutSpeed * Time.deltaTime);
                        stillActive = true;
                    }
                }
                else if(src.isPlaying)
                {
                    stillActive = true;
                }
            }

            if (stillActive)
            {
                yield return null;
            }

        } while (stillActive);
        onComplete?.Invoke();
    }
}
