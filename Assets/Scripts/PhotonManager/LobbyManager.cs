using UnityEngine;
using TMPro;
using Photon.Pun;
using NUnit.Framework;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private TMP_Text roomNameText;

    [Header("Player List")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;

    private List<GameObject> playerList = new List<GameObject>();

    private void OnEnable()
    {
        base.OnEnable();
        UpdatePlayerList();
    }
    private void OnDisable()
    {
        base.OnDisable();
        UpdatePlayerList();
    }
    void Start()
    {
        if(PhotonNetwork.InRoom)
        {
            string currentRoomName = PhotonNetwork.CurrentRoom.Name;
            roomNameText.text = currentRoomName;

            Debug.Log("Oda Adý Yazdýrýldý!");
        }
        else
        {
            Debug.Log("Bir Odada deðilsiniz!");
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log(newPlayer.NickName + " Katýldý!");
        UpdatePlayerList();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log(otherPlayer.NickName + " Odadan Ayrýldý!");
        UpdatePlayerList();
    }
    void UpdatePlayerList()
    {
        foreach(GameObject item in playerList)
        {
            Destroy(item);
        }
        playerList.Clear();

        if (!PhotonNetwork.InRoom) return;

        Player masterClient = null;

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            if(player.IsMasterClient)
            {
                masterClient = player;
                break;
            }
        }
        if(masterClient != null)
        {
            InstantiatePlayerListItem(masterClient);
        }

        foreach (Player player in PhotonNetwork.PlayerList)
        {
            if (!player.IsMasterClient)
            {
                InstantiatePlayerListItem(player);
            }
        }
    }
    void InstantiatePlayerListItem(Player player)
    {
        GameObject listItem = Instantiate(playerListItemPrefab, playerListContainer);

        TMP_Text textComponent = listItem.GetComponent<TMP_Text>();
        if (player.IsMasterClient)
        {
            textComponent.text = player.NickName + " (Kurucu)";
            textComponent.color = Color.yellow;
        }
        else
        {
            textComponent.text = player.NickName;
        }
        playerList.Add(listItem);
    }
    public void LeaveLobby()
    {
        Debug.Log("Lobiden Ayrýlýnýyor!");
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Odadan baþarýyla ayrýlýndý!");

        SceneManager.LoadScene("ClientScene");
    }
}
