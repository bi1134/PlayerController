using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public CinemachineVirtualCameraBase killCam;

    public void EnableKillCam()
    {
        killCam.Priority = 20;
    }
}
