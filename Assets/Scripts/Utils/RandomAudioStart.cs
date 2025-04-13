using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class RandomAudioStart : MonoBehaviour
{
    private AudioSource audioSource;
    
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.time = Random.Range(0, audioSource.clip.length);
    }
}
