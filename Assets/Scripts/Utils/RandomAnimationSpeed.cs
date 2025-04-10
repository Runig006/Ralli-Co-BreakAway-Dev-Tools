using UnityEngine;

public class RandomAnimationSpeed : MonoBehaviour
{
	[Range(0.0f,10.0f)] [SerializeField] private float minSpeed = 0.0f;
	[Range(0.0f,10.0f)] [SerializeField] private float maxSpeed = 1.0f;
	[SerializeField] private bool startInRandomPoint = true;
	void Start()
	{
		Animator animator = GetComponent<Animator>();
		if (animator != null)
		{
			animator.speed = Random.Range(this.minSpeed, this.maxSpeed);
			if(startInRandomPoint){
				animator.Play(0, 0, Random.Range(0.0f, 1.0f));
			}
		}
	}
}
