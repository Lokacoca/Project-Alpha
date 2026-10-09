using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class RoomList : MonoBehaviourPunCallbacks
{
    public static RoomList Instance;

    public GameObject roomManagerGameobject;
    public RoomManager roomManager;
    public GameObject LobbyGameObject;
    public GameObject ScoreManagerGameobject;



    [Header("UI")]
    public Transform roomListParent;
    public GameObject roomListItemPrefab;

    private List<RoomInfo> cachedRoomList = new List<RoomInfo>();


    public void ChangeRoomToCreateName(string _roomName)
    {
        roomManager.roomNameToJoin = _roomName;
    }




    private void Awake()
    {
        Instance = this;
    }

    IEnumerator Start()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            PhotonNetwork.Disconnect();
        }

        yield return new WaitUntil(() => !PhotonNetwork.IsConnected);
        PhotonNetwork.ConnectUsingSettings();
    }


    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        PhotonNetwork.JoinLobby();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (cachedRoomList.Count == 0)
        {
            cachedRoomList = new List<RoomInfo>(roomList);
        }
        else
        {
            foreach (var room in roomList)
            {
                bool roomFound = false;

                for (int i = 0; i < cachedRoomList.Count; i++)
                {
                    if (cachedRoomList[i].Name == room.Name)
                    {
                        roomFound = true;

                        if (room.RemovedFromList)
                        {
                            cachedRoomList.RemoveAt(i);
                        }
                        else
                        {
                            cachedRoomList[i] = room;
                        }

                        break;
                    }
                }

                if (!roomFound && !room.RemovedFromList)
                {
                    cachedRoomList.Add(room);
                }
            }
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        foreach (Transform roomItem in roomListParent)
        {
            Destroy(roomItem.gameObject);
        }

        foreach (var room in cachedRoomList)
        {
            GameObject roomItem = Instantiate(roomListItemPrefab, roomListParent);

            roomItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = room.Name;
            roomItem.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = room.PlayerCount + "/16";

            roomItem.GetComponent<RoomItemButton>().RoomName = room.Name;
        }
    }

    public void JoinRoomByName(string roomName)
    {
        roomManager.roomNameToJoin = roomName;
        roomManagerGameobject.SetActive(true);

        Debug.Log("Joining room: " + roomName);
        PhotonNetwork.JoinRoom(roomName);
        LobbyGameObject.SetActive(false);
        ScoreManagerGameobject.SetActive(true);
    }


    public void CreateRoomButton()
    {
        roomManager.roomNameToJoin = "test";
        roomManagerGameobject.SetActive(true);
        ScoreManagerGameobject.SetActive(true);
    }


}
