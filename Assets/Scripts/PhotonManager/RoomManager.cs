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
    private bool hasTriedJoinRoomFallback = false;

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

        //TMP_Text textComponent = listItem.GetComponent<TMP_Text>();
        TMP_Text nicknameText = listItem.transform.Find("NicknameText").GetComponent<TMP_Text>();
        TMP_Text eloText = listItem.transform.Find("EloText").GetComponent<TMP_Text>();
        if (player.IsMasterClient)
        {
            nicknameText.text = player.NickName + " (Kurucu)";
            nicknameText.color = Color.yellow;
        }
        else
        {
            nicknameText.text = player.NickName;
        }
        if(player.CustomProperties.TryGetValue("elo",out object eloValue))
        {
            eloText.text = ($"{eloValue}");
        }
        else
        {
            eloText.text = "?";
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
        hasTriedJoinRoomFallback = false;

        Debug.Log("Master'a baðlandý. Odaya tekrar girilmeye çalýþýlýyor...");
        if (!string.IsNullOrEmpty(lastKnownRoomName))
        {
            PhotonNetwork.RejoinRoom(lastKnownRoomName);
        }
        else
        {
            Debug.LogWarning("Geri dönülecek oda ismi bulunamadý. Ana menüye yönlendiriliyor.");
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
        Debug.LogError($"Odaya geri dönülemedi (Rejoin). Hata: {message}");

        if (!hasTriedJoinRoomFallback)
        {
            Debug.Log("Rejoin baþarýsýz oldu, JoinRoom deneniyor...");
            hasTriedJoinRoomFallback = true;
            if (!string.IsNullOrEmpty(lastKnownRoomName))
            {
                PhotonNetwork.JoinRoom(lastKnownRoomName);
            }
            else
            {
                Debug.LogError("Oda ismi bilinmiyor. Yedek deneme yapýlamýyor. ClientScene'e dönülüyor.");
                SceneManager.LoadScene("ClientScene");
            }
        }
        else
        {
            Debug.LogError("JoinRoom denemesi de baþarýsýz oldu. ClientScene'e dönülüyor.");
            SceneManager.LoadScene("ClientScene");
        }
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
