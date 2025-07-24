using Fusion;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FusionNetworkManager : MonoBehaviour
{
    public static FusionNetworkManager Instance;

    [Header("Fusion")]
    public NetworkRunner networkRunnerPrefab;
    private NetworkRunner networkRunnerInstance;

    [Header("Scene Management")]
    public string gameSceneName = "Lobby";

    [Header("Callbacks")]
    public NetworkPlayerSpawner playerSpawner; // Add this in the Inspector

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private async void Start()
    {
        networkRunnerInstance = Instantiate(networkRunnerPrefab);
        networkRunnerInstance.name = "NetworkRunner";
        networkRunnerInstance.ProvideInput = true;

        var sceneManager = networkRunnerInstance.gameObject.AddComponent<NetworkSceneManagerDefault>();
        var sceneRef = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);

        // Register the spawner for OnPlayerJoined and other callbacks
        networkRunnerInstance.AddCallbacks(playerSpawner);

        var result = await networkRunnerInstance.StartGame(new StartGameArgs
        {
            GameMode = GameMode.Shared,
            SessionName = "FusionRoom",
            Scene = sceneRef,
            SceneManager = sceneManager
        });

        if (result.Ok)
        {
            Debug.Log("Session started successfully.");
        }
        else
        {
            Debug.LogError("Failed to start session: " + result.ShutdownReason);
        }
    }
}
