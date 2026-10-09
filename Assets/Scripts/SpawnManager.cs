using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnManager : MonoBehaviourPunCallbacks
{

    [Header("RESOURCES")]
    public Transform spawnPoint;
    public GameObject playerPrefab;
    

    void Start()
    {
        SpawnPlayer();
    }


    void SpawnPlayer()
    {
      

     
       

        //PhotonNetwork.Instantiate(playerPrefab.name, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("SPAWNED");
    }
   
    void Update()
    {
        
    }
}
