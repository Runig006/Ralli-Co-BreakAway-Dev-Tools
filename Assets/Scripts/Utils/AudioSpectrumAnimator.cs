using UnityEngine;

public class AudioSpectrumAnimator : MonoBehaviour
{
	private AudioVisualizer audioVisualizer;
	private Animator animator;

	[SerializeField] private FrequencyRange frequencyRange = FrequencyRange.Mid;
	
	[Range(0,511)][SerializeField] private int minBandIndex = 0;
	[Range(0,511)][SerializeField] private int maxBandIndex = 0; 
	[SerializeField] private float intensityMultiplier = 1f;
	[SerializeField] private float smoothTime = 0.1f;
	[SerializeField] private string animationParameter = "SpectrumValue";
	
	[SerializeField] private bool enableCulling = true;
	
	private float targetValue = 0f;
	private float currentValue = 0f;
	private float velocity = 0f;

	void Start()
	{
		this.audioVisualizer = FindFirstObjectByType<AudioVisualizer>();
		this.animator = GetComponent<Animator>();
	}
	
	void OnBecameInvisible()
	{
		if(this.enableCulling){
			this.enabled = false;
			this.animator.enabled = false;
		}
	}

	void OnBecameVisible()
	{
		if(this.enableCulling){
			this.enabled = true;
			this.animator.enabled = true;
		}
	}


	void Update()
	{
		if (audioVisualizer == null || animator == null) {
			return;
		}

		if (frequencyRange == FrequencyRange.Custom)
		{
			float[] spectrumData = audioVisualizer.GetSpectrumData();
			if (spectrumData == null || spectrumData.Length <= maxBandIndex) {
				return;
			}

			float sum = 0f;
			for (int i = minBandIndex; i <= maxBandIndex; i++)
			{
				sum += spectrumData[i];
			}
			this.targetValue = sum / maxBandIndex - minBandIndex + 1;
		}
		else
		{
			this.targetValue = audioVisualizer.GetPrecomputedRange(frequencyRange);
		}
		
		this.targetValue = Mathf.Clamp01(this.targetValue * intensityMultiplier);
		this.currentValue = Mathf.Lerp(this.currentValue, this.targetValue, Time.deltaTime / this.smoothTime);
		animator.SetFloat(this.animationParameter, this.currentValue);
	}
}
