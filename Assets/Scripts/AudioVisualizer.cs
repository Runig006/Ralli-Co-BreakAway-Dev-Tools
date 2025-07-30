using UnityEngine;

public enum FrequencyRange { SubBass, Bass, LowMid, Mid, HighMid, Treble, Custom }

public class AudioVisualizer : MonoBehaviour
{	
	private AudioSource audioSource; 
	private float[] spectrumData = new float[512];
	private float[] precomputedRanges = new float[6];

	void Start()
	{
		this.audioSource = GetComponent<AudioSource>();
	}
	
	public float[] GetSpectrumData()
	{
		return this.spectrumData;
	}
	
	public float GetPrecomputedRange(FrequencyRange range)
	{
		if (range == FrequencyRange.Custom)
		{
			return 0f;
		}
		
		return precomputedRanges[(int)range];
	}

	void FixedUpdate()
	{
		this.audioSource.GetSpectrumData(this.spectrumData, 0, FFTWindow.BlackmanHarris);
		this.PrecomputeRanges();
	}
	
	void PrecomputeRanges()
	{
		precomputedRanges[0] = ComputeRange(0, 10);
		precomputedRanges[1] = ComputeRange(10, 40);
		precomputedRanges[2] = ComputeRange(40, 80);
		precomputedRanges[3] = ComputeRange(80, 200);
		precomputedRanges[4] = ComputeRange(200, 300);
		precomputedRanges[5] = ComputeRange(300, 450);
	}

	float ComputeRange(int minIndex, int maxIndex)
	{
		float sum = 0f;
		int count = maxIndex - minIndex + 1;
		for (int i = minIndex; i <= maxIndex; i++)
		{
			sum += spectrumData[i];
		}
		return sum / count;
	}
}
