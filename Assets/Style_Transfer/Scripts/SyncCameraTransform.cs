using UnityEngine;

public class SyncCameraTransform : MonoBehaviour
{
    public Camera mainCamera; // Esta es la cámara real, con el CinemachineBrain

    void LateUpdate()
    {
        if (mainCamera != null && mainCamera.isActiveAndEnabled)
        {
            transform.position = mainCamera.transform.position;
            transform.rotation = mainCamera.transform.rotation;
            GetComponent<Camera>().fieldOfView = mainCamera.fieldOfView;
        }
    }
}
