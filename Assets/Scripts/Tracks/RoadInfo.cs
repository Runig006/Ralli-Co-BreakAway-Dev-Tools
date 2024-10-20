using UnityEngine;

public class RoadInfo : MonoBehaviour
{
	[SerializeField] private int lanesNumber = 4;
	
	public int GetLanesNumber()
	{
		return this.lanesNumber;
	}
}