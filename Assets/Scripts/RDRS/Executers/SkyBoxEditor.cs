using UnityEngine;

public class SkyBoxEditor : RDRSNodeWithFrequency
{
    [SerializeField] private RDRSNode shouldApplyReader;
    [SerializeField] private RDRSNode skyBoxReader;
    [SerializeField] private bool restoreWhenFalse = false;

    private bool lastChecker = false;
    private SkyBoxInfo previousSkyBox;
    private SkyBoxInfo currentSkyBox;

    #if FULL_GAME
    private SkyBoxConnector skyBoxConnector;

    void Awake()
    {
        this.skyBoxConnector = UnityEngine.Object.FindFirstObjectByType<SkyBoxConnector>();
    }
    #endif


    public override void Execute()
    {
        #if FULL_GAME
        if (this.skyBoxConnector == null || this.skyBoxReader == null || this.shouldApplyReader == null)
        {
            return;
        }

        bool shouldApply = RDRSUtils.toBoolean(this.shouldApplyReader?.GetValue());

        if (shouldApply == this.lastChecker && this.HasSkyBoxChanged() == false)
        {
            return;
        }

        if (shouldApply == false)
        {
            this.RestorePreviousSkyBoxIfNeeded();
        }
        else
        {
            this.ApplyNewSkyBoxIfNeeded();
        }

        this.lastChecker = shouldApply;
        #endif
    }

    #if FULL_GAME
    private bool HasSkyBoxChanged()
    {
        object rawValue = this.skyBoxReader.GetValue();
        SkyBoxInfo newSkyBox = this.ExtractSkyBoxInfo(rawValue);
        return newSkyBox != null && newSkyBox != this.currentSkyBox;
    }

    private void ApplyNewSkyBoxIfNeeded()
    {
        object rawValue = this.skyBoxReader.GetValue();
        SkyBoxInfo toApply = this.ExtractSkyBoxInfo(rawValue);

        if (toApply == null)
        {
            return;
        }

        if (this.currentSkyBox == null)
        {
            this.previousSkyBox = this.skyBoxConnector.GetCurrentSkyBoxInfo();
        }

        if (toApply != this.currentSkyBox)
        {
            this.skyBoxConnector.SetSkyBox(toApply);
            this.currentSkyBox = toApply;
        }
    }

    private void RestorePreviousSkyBoxIfNeeded()
    {
        if (this.restoreWhenFalse && this.currentSkyBox != null && this.previousSkyBox != null)
        {
            this.skyBoxConnector.SetSkyBox(this.previousSkyBox);
            this.currentSkyBox = null;
        }
    }

    private SkyBoxInfo ExtractSkyBoxInfo(object source)
    {
        if (source is SkyBoxInfo info)
        {
            return info;
        }

        if (source is GameObject go)
        {
            return go.GetComponentInChildren<SkyBoxInfo>();
        }

        return null;
    }
    #endif
}
