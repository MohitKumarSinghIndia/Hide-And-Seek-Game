using Fusion;
using System.Linq;

public class SessionInfoNetwork : NetworkBehaviour
{
    [Networked] public int PlayerCount { get; set; }
    [Networked] public NetworkString<_64> SessionName { get; set; }
    [Networked] public NetworkString<_16> Region { get; set; }

    public static SessionInfoNetwork Instance;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
        {
            SessionName = Runner.SessionInfo?.Name ?? "None";
            Region = Runner.SessionInfo?.Region ?? "N/A";
        }

        Instance = this;
    }

    public override void FixedUpdateNetwork()
    {
        if (Object.HasStateAuthority)
        {
            PlayerCount = Runner.ActivePlayers.Count();
        }
    }

    private void Update()
    {
        SessionUIController.Instance?.SetSessionInfo(this);
    }
}