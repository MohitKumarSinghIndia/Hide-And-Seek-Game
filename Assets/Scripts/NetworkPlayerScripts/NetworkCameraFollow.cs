using ReadyPlayerMe.Core;
using ReadyPlayerMe.Samples.QuickStart;
using UnityEngine;

public class NetworkCameraFollow : MonoBehaviour
{
    private const string TARGET_NOT_SET = "Target not set, disabling component";
    private readonly string TAG = typeof(CameraFollow).ToString();
    [SerializeField]
    [Tooltip("The camera that will follow the target")]
    public Camera playerCamera;
    [Tooltip("The target Transform (GameObject) to follow")]
    public Transform target;
    [SerializeField]
    [Tooltip("Defines the camera distance from the player along Z (forward) axis. Value should be negative to position behind the player")]
    private float cameraDistance = -3f;
    [SerializeField] private bool followOnStart = true;
    private bool isFollowing;

    private void LateUpdate()
    {
        if (isFollowing)
        {
            playerCamera.transform.localPosition = Vector3.forward * cameraDistance;
            playerCamera.transform.localRotation = Quaternion.Euler(Vector3.zero);
            transform.position = target.position;
        }
    }
    public void SetTarget(Transform newTarget)
    {
        if (newTarget == null)
        {
            Debug.LogWarning(TAG + ": " + TARGET_NOT_SET);
            enabled = false;
            return;
        }
        target = newTarget;
        if (followOnStart)
        {
            StartFollow();
        }
    }
    public void StopFollow()
    {
        isFollowing = false;
    }

    public void StartFollow()
    {
        isFollowing = true;
    }
}
