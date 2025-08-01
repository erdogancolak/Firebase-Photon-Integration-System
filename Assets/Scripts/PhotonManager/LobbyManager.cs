using UnityEngine;
using TMPro;
using Photon.Pun;
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

    private bool isLeavingManually = false;

    void Start()
    {
        UpdatePlayerList();

        GameStateManager.LastSceneName = "LobbyScene";
        GameStateManager.LastRoomName = PhotonNetwork.CurrentRoom.Name;
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
        if (PhotonNetwork.CurrentRoom != null)
            roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        foreach(GameObject item in playerList)
        {
            Destroy(item);
        }
        playerList.Clear();

        foreach(Player player in PhotonNetwork.PlayerList)
        {
            InstantiatePlayerListItem(player);
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
        Debug.Log("Lobiden Manuel olarak Ayrýlýnýyor!");

        isLeavingManually = true;
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        if (isLeavingManually)
        {
            Debug.Log("Odadan baþarýyla manuel olarak ayrýlýndý! ClientScene yükleniyor.");

            isLeavingManually = false;

            SceneManager.LoadScene("ClientScene");
        }
    }
}
