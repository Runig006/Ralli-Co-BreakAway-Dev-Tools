using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
	private CinemachineCamera[] cams;
	private CinemachineCamera currentCam;
	private CameraPosition cameraPosition;
	
	
	public void Awake()
	{
		this.cams = GetComponentsInChildren<CinemachineCamera>();
	}
	
	public bool GetIsInternalCamera()
	{
		return this.cameraPosition != null ? this.cameraPosition.GetInterior() : false;
	}
	
	public float GetMasterVolume()
	{
		return this.cameraPosition != null ? this.cameraPosition.GetMasterVolume() : 1.0f;
	}
	
	//Cameras
	public void SetCamera(CinemachineCamera currentCam)
	{
		foreach(CinemachineCamera cam in this.cams)
		{
			cam.Priority = 0;
		}
		this.currentCam = currentCam;
		this.cameraPosition = this.currentCam.gameObject.GetComponent<CameraPosition>();
		this.currentCam.Priority = 10;
	}
	
	public void NextCamera()
	{
		int currentIndex = System.Array.IndexOf(this.cams, this.currentCam);
		int nextIndex = (currentIndex + 1) % this.cams.Length; 
		SetCamera(this.cams[nextIndex]);
	}
}
