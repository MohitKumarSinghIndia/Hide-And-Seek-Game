using Fusion;
using UnityEngine;

public class NetworkThirdPersonController : NetworkBehaviour
{
    private const float FALL_TIMEOUT = 0.15f;

    // Animator parameter hashes
    private static readonly int MoveSpeedHash = Animator.StringToHash("MoveSpeed");
    private static readonly int MotionSpeedHash = Animator.StringToHash("MotionSpeed");
    private static readonly int JumpHash = Animator.StringToHash("JumpTrigger");
    private static readonly int FreeFallHash = Animator.StringToHash("FreeFall");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");

    private GameObject avatar;
    private NetworkThirdPersonMovement thirdPersonMovement;
    private NetworkPlayerInput playerInput;

    private Animator animator;
    private float fallTimeoutDelta;
    private bool isInitialized;

    [SerializeField]
    [Tooltip("Toggle input detection (for editor testing)")]
    private bool inputEnabled = true;

    public void Setup(GameObject target, RuntimeAnimatorController runtimeAnimatorController)
    {
        if (!isInitialized)
        {
            Init();
        }

        avatar = target;
        thirdPersonMovement.Setup(avatar);

        animator = avatar.GetComponent<Animator>();
        animator.runtimeAnimatorController = runtimeAnimatorController;
        animator.applyRootMotion = false;
    }

    private void Init()
    {
        thirdPersonMovement = GetComponent<NetworkThirdPersonMovement>();
        playerInput = GetComponent<NetworkPlayerInput>();

        if (playerInput != null)
        {
            playerInput.OnJumpPress += OnJump;
        }

        isInitialized = true;
    }
    private void Update()
    {
        if (!HasInputAuthority) return;

        if (inputEnabled && playerInput != null)
        {
            playerInput.CheckInput();

            var xAxisInput = playerInput.AxisHorizontal;
            var yAxisInput = playerInput.AxisVertical;

            thirdPersonMovement.Move(xAxisInput, yAxisInput);
            thirdPersonMovement.SetIsRunning(playerInput.IsHoldingLeftShift);
        }

        if (isInitialized)
        {
            UpdateAnimator();
        }
    }

    private void UpdateAnimator()
    {
        if (!HasInputAuthority || animator == null) return;

        var isGrounded = thirdPersonMovement.IsGrounded();

        animator.SetFloat(MoveSpeedHash, thirdPersonMovement.CurrentMoveSpeed);
        animator.SetBool(IsGroundedHash, isGrounded);

        if (isGrounded)
        {
            fallTimeoutDelta = FALL_TIMEOUT;
            animator.SetBool(FreeFallHash, false);
        }
        else
        {
            if (fallTimeoutDelta >= 0.0f)
            {
                fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                animator.SetBool(FreeFallHash, true);
            }
        }
    }

    private void OnJump()
    {
        if (!HasInputAuthority || animator == null) return;

        if (thirdPersonMovement.TryJump())
        {
            animator.SetTrigger(JumpHash);
        }
    }
}
