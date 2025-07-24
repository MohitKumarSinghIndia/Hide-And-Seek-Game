using Fusion;
using ReadyPlayerMe.Samples.QuickStart;
using UnityEngine;

public class NetworkThirdPersonMovement : NetworkBehaviour
{
    // === Constants ===
    private const float TURN_SMOOTH_TIME = 0.05f;

    // === Public References ===
    public Transform cameraTarget;
    public GameObject cameraPrefab;
    private GameObject cameraInstance;
    // === Serialized Settings ===
    [SerializeField, Tooltip("Move speed of the character")]
    private float walkSpeed = 3f;

    [SerializeField, Tooltip("Run speed of the character")]
    private float runSpeed = 8f;

    [SerializeField, Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    private float gravity = -18f;

    [SerializeField, Tooltip("The height the player can jump")]
    private float jumpHeight = 3f;

    // === Private Runtime References ===
    public Transform playerCamera;
    private CharacterController controller;
    private NetworkGroundCheck groundCheck;
    private GameObject avatar;

    // === Movement State ===
    private float verticalVelocity;
    private float turnSmoothVelocity;
    private bool jumpTrigger;
    private bool isRunning;
    public float CurrentMoveSpeed { get; private set; }

    // === Avatar Rotation ===
    [Networked, OnChangedRender(nameof(OnAvatarRotationChanged))]
    private Quaternion AvatarRotation { get; set; }

    private Quaternion desiredAvatarRotation = Quaternion.identity;
    private bool shouldRotateAvatar = false;

    public override void Spawned()
    {
        controller = GetComponent<CharacterController>();
        groundCheck = GetComponent<NetworkGroundCheck>();

        if (HasInputAuthority)
        {
            SetupCamera();
        }
    }

    public void Setup(GameObject target)
    {
        avatar = target;

        // Apply rotation to remote avatar
        if (!HasInputAuthority && avatar != null)
        {
            avatar.transform.rotation = AvatarRotation;
        }
    }

    private void SetupCamera()
    {
        if (cameraPrefab == null)
        {
            Debug.LogWarning("Camera prefab not assigned.");
            return;
        }

        cameraInstance = Instantiate(cameraPrefab, Vector3.zero, Quaternion.identity);

        NetworkCameraOrbit networkCameraOrbit = cameraInstance.GetComponent<NetworkCameraOrbit>();
        NetworkCameraFollow networkCameraFollow = cameraInstance.GetComponent<NetworkCameraFollow>();

        if (networkCameraOrbit != null && networkCameraFollow != null)
        {
            networkCameraOrbit.SetPlayerInput(GetComponent<NetworkPlayerInput>());
            networkCameraFollow.SetTarget(cameraTarget);
            playerCamera = cameraInstance.transform.GetChild(0); // Assuming the camera is the first child of the prefab
        }
        else
        {
            Debug.LogWarning("Camera prefab is missing required components.");
        }
    }

    public void Move(float inputX, float inputY)
    {
        if (!HasInputAuthority) return;

        var moveDirection = playerCamera.right * inputX + playerCamera.forward * inputY;
        var moveSpeed = isRunning ? runSpeed : walkSpeed;

        JumpAndGravity();

        controller.Move(
            moveDirection.normalized * (moveSpeed * Time.deltaTime) +
            new Vector3(0.0f, verticalVelocity * Time.deltaTime, 0.0f)
        );

        var moveMagnitude = moveDirection.magnitude;
        CurrentMoveSpeed = isRunning ? runSpeed * moveMagnitude : walkSpeed * moveMagnitude;

        if (moveMagnitude > 0)
        {
            RotateAvatarTowardsMoveDirection(moveDirection);
        }
    }

    private void RotateAvatarTowardsMoveDirection(Vector3 moveDirection)
    {
        if (!HasInputAuthority || avatar == null) return;

        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg;
        float angle = Mathf.SmoothDampAngle(avatar.transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, TURN_SMOOTH_TIME);

        desiredAvatarRotation = Quaternion.Euler(0, angle, 0);
        shouldRotateAvatar = true;

        // Apply immediately for local owner
        avatar.transform.rotation = desiredAvatarRotation;
    }

    public override void FixedUpdateNetwork()
    {
        if (HasInputAuthority && shouldRotateAvatar)
        {
            AvatarRotation = desiredAvatarRotation;
            shouldRotateAvatar = false;
        }
    }

    private void OnAvatarRotationChanged()
    {
        if (HasInputAuthority || avatar == null) return;

        avatar.transform.rotation = AvatarRotation;
    }

    private void JumpAndGravity()
    {
        if (!HasInputAuthority) return;

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        if (jumpTrigger && controller.isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpTrigger = false;
        }

        verticalVelocity += gravity * Time.deltaTime;
    }

    public void SetIsRunning(bool running)
    {
        isRunning = running;
    }

    public bool TryJump()
    {
        if (!HasInputAuthority) return false;

        jumpTrigger = false;
        if (controller.isGrounded)
        {
            jumpTrigger = true;
        }
        return jumpTrigger;
    }

    public bool IsGrounded()
    {
        if (verticalVelocity > 0)
        {
            return false;
        }

        return groundCheck.IsGrounded();
    }
}