using UnityEngine;

public class AudioSpectrumReader : RDRSNode
{
    [SerializeField] private FrequencyRange frequencyRange = FrequencyRange.Mid;

    [SerializeField][Tooltip("Leave empty for using the 'playerMusic'")] AudioVisualizer audioVisualizer;

    [Range(0, 511)][SerializeField] private int minBandIndex = 0;
    [Range(0, 511)][SerializeField] private int maxBandIndex = 0;

    [SerializeField] private bool getRawValue = false;
    [SerializeField] private float minValue = 0f;
    [SerializeField] private float maxValue = 0f;

    static AudioVisualizer radioAudioVisualizer;

    private void OnValidate()
    {
        switch (this.frequencyRange)
        {
            case FrequencyRange.SubBass:
                this.maxValue = 0.03f;
                break;
            case FrequencyRange.Bass:
                this.maxValue = 0.015f;
                break;
            case FrequencyRange.LowMid:
                this.maxValue = 0.008f;
                break;
            case FrequencyRange.Mid:
                this.maxValue = 0.004f;
                break;
            case FrequencyRange.HighMid:
                this.maxValue = 0.002f;
                break;
            case FrequencyRange.Treble:
                this.maxValue = 0.0002f;
                break;
        }
    }

    public void findAudioVisualizer()
    {
        if(this.audioVisualizer != null)
        {
            return;
        }
        if(radioAudioVisualizer == null)
        {
            radioAudioVisualizer = FindFirstObjectByType<AudioVisualizer>();
            Debug.Log(radioAudioVisualizer);
        }
        this.audioVisualizer = radioAudioVisualizer;
    }

    public override object GetValue()
    {
        if (this.audioVisualizer == null)
        {
            this.findAudioVisualizer();
            return 0.0f;
        }

        float value = 0f;

        if (this.frequencyRange == FrequencyRange.Custom)
        {
            float[] spectrum = this.audioVisualizer.GetSpectrumData();
            if (spectrum == null || spectrum.Length <= maxBandIndex || minBandIndex > maxBandIndex)
            {
                return 0.0f;
            }

            float sum = 0f;
            for (int i = minBandIndex; i <= maxBandIndex; i++)
            {
                sum += spectrum[i];
            }

            int bandCount = Mathf.Max(1, maxBandIndex - minBandIndex + 1);
            value = sum / bandCount;
        }
        else
        {
            value = this.audioVisualizer.GetPrecomputedRange(frequencyRange);
        }

        if (this.getRawValue)
        {
            return value;
        }
        else
        {
            return Mathf.InverseLerp(this.minValue, this.maxValue, value);
        }
    }
}
