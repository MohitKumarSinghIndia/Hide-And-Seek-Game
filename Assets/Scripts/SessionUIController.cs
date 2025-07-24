using Fusion;
using TMPro;
using UnityEngine;

public class SessionUIController : MonoBehaviour
{
    public static SessionUIController Instance;

    public TMP_Text playerCountText;
    public TMP_Text hostText;
    public TMP_Text sessionNameText;
    public TMP_Text regionText;

    private void Awake()
    {
        Instance = this;
    }

    public void SetSessionInfo(SessionInfoNetwork info)
    {
        playerCountText.text = $"Players: {info.PlayerCount}";

        if (info.Object != null && info.Object.HasInputAuthority && info.Runner != null)
        {
            hostText.text = info.Runner.IsSharedModeMasterClient
                ? "You are the Host"
                : "You are a Client";
        }

        sessionNameText.text = $"Room: {info.SessionName.ToString()}";
        regionText.text = $"Region: {info.Region.ToString()}";
    }
}
