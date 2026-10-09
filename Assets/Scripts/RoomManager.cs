using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;


public class RoomManager : MonoBehaviourPunCallbacks
{
    [Header("Spawn Points")]
    public Transform singlePlayerSpawn;   // Waiting area
    public Transform multiPlayerSpawn;

    
    public GameObject SettingsPage;
    public GameObject PlayerUI;




    // Match area

    [Header("Room Settings")]
    public string roomNameToJoin = "test";
    public byte maxPlayers = 16;

    public GameObject PlayerGameobject;

    public static RoomManager Instance;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.LeaveLobby();
            PhotonNetwork.LeaveRoom();
        }

        if (!PhotonNetwork.IsConnected)
        {
            Debug.Log("RoomManager: Connecting to Photon...");
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.Log("RoomManager: Already connected, joining lobby...");
            PhotonNetwork.JoinLobby();
            
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✅ Connected to Master Server");
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✅ Joined Lobby");

        if (!string.IsNullOrEmpty(roomNameToJoin))
        {
            Debug.Log($"🔹 Attempting to join or create room: {roomNameToJoin}");
            RoomOptions options = new RoomOptions();
            options.MaxPlayers = maxPlayers;
            PhotonNetwork.JoinOrCreateRoom(roomNameToJoin, options, TypedLobby.Default);
        }
        else
        {
            Debug.LogWarning("⚠️ No room name provided!");
        }
    }

    public override void OnJoinedRoom()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.GameOver = false;
            Debug.Log("🎮 GameOver reset for new match");
        }

        // Spawn player
        Transform spawn = PhotonNetwork.CurrentRoom.PlayerCount < 2
            ? singlePlayerSpawn
            : multiPlayerSpawn;

        PlayerGameobject = PhotonNetwork.Instantiate("Player", spawn.position, spawn.rotation);
        PlayerUI.SetActive(true);

    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            photonView.RPC("ForceRespawnAllPlayers", RpcTarget.All);
        }
    }

    [PunRPC]
    private void ForceRespawnAllPlayers()
    {
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return null;

        if (PlayerGameobject != null)
        {
            PhotonNetwork.Destroy(PlayerGameobject);
        }

        Transform spawn = multiPlayerSpawn;

        PlayerGameobject = PhotonNetwork.Instantiate("Player", spawn.position, spawn.rotation);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newState = !SettingsPage.activeSelf;
            SettingsPage.SetActive(newState);

            GameState.InMenu = newState;

        }
    }

}
