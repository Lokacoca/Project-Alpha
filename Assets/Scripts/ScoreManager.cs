using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    public GameObject PlayerPrefab;
    public GameObject LobbyGameObject;

    public GameObject RoomManagerGameObject;

    public TextMeshProUGUI MyScoreText;
    public TextMeshProUGUI EnemyScoreText;

    public GameObject CreateRoomScreenGameObject;
    public GameObject JoinGameScreenGameObject;

    public GameObject LoseUIGameObject;
    public GameObject WinUIGameObject;

    public bool GameOver { get; set; } = false;

    private int myScore = 0;
    private int enemyScore = 0;

    void Awake()
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

        UpdateUI();
    }



    public void AddKill()
    {
        myScore++;
        UpdateUI();
    }

    public void AddDeath()
    {
        enemyScore++;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (MyScoreText != null)
            MyScoreText.text = $"{myScore}";
        if (EnemyScoreText != null)
            EnemyScoreText.text = $"{enemyScore}";

        if (enemyScore >= 12)
            StartCoroutine(ShowLoseUI());
        if (myScore >= 12)
            StartCoroutine(ShowWinUi());
    }

    private IEnumerator ShowLoseUI()
    {
        GameOver = true; // Mark game as ended
        LoseUIGameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();


        StartCoroutine(ReturnToLobby());
    }

    private IEnumerator ShowWinUi()
    {
        GameOver = true; // Mark game as ended
        WinUIGameObject.SetActive(true);
        yield return new WaitForSeconds(2f);

        if (PhotonNetwork.InRoom)
            PhotonNetwork.LeaveRoom();

        ;

        StartCoroutine(ReturnToLobby());
    }


    private IEnumerator ReturnToLobby()
    {
        yield return new WaitUntil(() => !PhotonNetwork.InRoom);

        WinUIGameObject.SetActive(false);
        LoseUIGameObject.SetActive(false);

        LobbyGameObject.SetActive(true);
        CreateRoomScreenGameObject.SetActive(false);
        JoinGameScreenGameObject.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }




}
