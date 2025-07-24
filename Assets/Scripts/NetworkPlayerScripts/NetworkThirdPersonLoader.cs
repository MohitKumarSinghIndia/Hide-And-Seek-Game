using Fusion;
using ReadyPlayerMe.Core;
using System;
using TMPro;
using UnityEngine;

public class NetworkThirdPersonLoader : NetworkBehaviour
{
    private readonly Vector3 avatarPositionOffset = new Vector3(0, -0.08f, 0);
    private GameObject avatar;
    private GameObject previewAvatar;
    private AvatarObjectLoader avatarObjectLoader;
    
    public TMP_Text playerNameText;
    public GameObject nameCanvas;
    [Networked] public NetworkString<_512> AvatarUrl { get; set; }
    [Networked] public NetworkString<_512> PlayerName { get; set; }

    [SerializeField] public bool loadOnStart = true;
    [SerializeField] private RuntimeAnimatorController animatorController;
    [SerializeField] private NetworkMecanimAnimator networkMecanimAnimator;
    [SerializeField] private Animator referenceAnimator;

    public event Action OnLoadComplete;

    public override void Spawned()
    {
        avatarObjectLoader = new AvatarObjectLoader();
        avatarObjectLoader.OnCompleted += OnLoadCompleted;
        avatarObjectLoader.OnFailed += OnLoadFailed;

        if (previewAvatar != null)
        {
            SetupAvatar(previewAvatar);
        }

        LoadAvatar(AvatarUrl.ToString());
    }

    private void OnLoadFailed(object sender, FailureEventArgs args)
    {
        Debug.LogError($"Avatar load failed: {args.Message}");
        OnLoadComplete?.Invoke();
    }

    private void OnLoadCompleted(object sender, CompletionEventArgs args)
    {
        if (previewAvatar != null)
        {
            Destroy(previewAvatar);
            previewAvatar = null;
        }

        SetupAvatar(args.Avatar);
        OnLoadComplete?.Invoke();
    }

    private void SetupAvatar(GameObject targetAvatar)
    {
        if (avatar != null)
        {
            Destroy(avatar);
        }

        avatar = targetAvatar;
        avatar.transform.parent = transform;
        avatar.transform.localPosition = avatarPositionOffset;
        avatar.transform.localRotation = Quaternion.identity;

        Animator animator = avatar.GetComponent<Animator>();

        if (animator == null)
        {
            animator = avatar.AddComponent<Animator>();
        }

        if (animatorController != null)
        {
            animator.runtimeAnimatorController = animatorController;
        }

        if (networkMecanimAnimator != null)
        {
            networkMecanimAnimator.Animator = animator;
            referenceAnimator.enabled = false;
            networkMecanimAnimator.enabled = true;
        }

        var controller = GetComponent<NetworkThirdPersonController>();
        if (controller != null)
        {
            controller.Setup(avatar, animatorController);
        }

        if(nameCanvas != null)
        {
            nameCanvas.SetActive(true);
        }
    }

    public void LoadAvatar(string url)
    {
        AvatarUrl = url.Trim();
        avatarObjectLoader.LoadAvatar(AvatarUrl.ToString());
    }

    private void Update()
    {
        if (nameCanvas != null && nameCanvas.activeSelf)
        {
            playerNameText.text = PlayerName.ToString();
            nameCanvas.transform.LookAt(Camera.main.transform);
        }
    }

}
