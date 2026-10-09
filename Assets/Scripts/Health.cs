using UnityEngine;
using Photon.Pun;

public class Health : MonoBehaviourPun
{
    public int maxHealth = 3;
    private int currentHealth;

    public GameObject PlayerPrefab;
    public Transform spawnPoint;


    void Start()
    {
        currentHealth = maxHealth;
    }

    [PunRPC]
    public void RPC_TakeDamage(int amount, int attackerID)
    {
        currentHealth -= amount;

        if (photonView.IsMine)
            DamageFogController.Instance.RegisterDamage();

        if (currentHealth <= 0)
        {
            Die(attackerID);
        }
    }


    private void Die(int attackerID)
    {
        if (!photonView.IsMine)
            return;

        if (ScoreManager.Instance != null)
            ScoreManager.Instance.AddDeath();

        photonView.RPC(nameof(RPC_AddKillToAttacker), RpcTarget.All, attackerID);

        PhotonNetwork.Destroy(gameObject);

        // ✅ Only respawn if game isn't over and still in room
        if (ScoreManager.Instance != null &&
            !ScoreManager.Instance.GameOver &&
            PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate("Player", spawnPoint.position, spawnPoint.rotation);
        }
    }


    [PunRPC]
    void RPC_AddKillToAttacker(int attackerID)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == attackerID)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.AddKill();
        }
    }
}
