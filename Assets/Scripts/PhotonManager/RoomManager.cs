using UnityEngine;
using TMPro;
using Photon.Pun;
using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine.SceneManagement;

public class RoomManager : MonoBehaviourPunCallbacks
{
    public static RoomManager instance {  get; private set; }
    [Header("UI References")]
    [SerializeField] private TMP_Text roomNameText;

    [Header("Player List")]
    [SerializeField] private Transform playerListContainer;
    [SerializeField] private GameObject playerListItemPrefab;
    private List<GameObject> playerList = new List<GameObject>();

    private bool isLeavingManually = false;
    private string lastKnownRoomName;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }
    void Start()
    {
        UpdatePlayerList();

        GameStateManager.LastSceneName = SceneManager.GetActiveScene().name;
        if (PhotonNetwork.CurrentRoom != null)
        {
            lastKnownRoomName = PhotonNetwork.CurrentRoom.Name;
            GameStateManager.LastRoomName = lastKnownRoomName;
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
    public void ReconnectAndRejoin()
    {
        Debug.Log("Yeniden baðlanýlýyor ve odaya dönülmeye çalýþýlýyor...");
        if(!PhotonNetwork.IsConnected)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public override void OnConnectedToMaster()
    {
        if (!string.IsNullOrEmpty(lastKnownRoomName))
        {
            Debug.Log($"Master'a baðlandý. Odaya tekrar giriliyor: {lastKnownRoomName}");
            PhotonNetwork.RejoinRoom(lastKnownRoomName);
        }
        else
        {
            SceneManager.LoadScene("ClientScene");
        }
    }
    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya baþarýyla girildi/geri dönüldü.");

        UpdatePlayerList(); 
    }
    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Odaya geri dönülemedi: {message}. Ana menüye yönlendiriliyor.");

        SceneManager.LoadScene("ClientScene");
    }
    public void LeaveRoomButton()
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
    public override void OnDisconnected(DisconnectCause cause)
    {
        if (!isLeavingManually && !GameStateManager.IsNoInternetPanelActive)
        {
            Debug.LogError("Beklenmedik bir þekilde baðlantý koptu! Sebep: " + cause);
        }
        else if (GameStateManager.IsNoInternetPanelActive)
        {
            Debug.LogWarning("Ýnternet baðlantýsý nedeniyle Photon baðlantýsý koptu. Panel aktif.");
        }
    }
}
