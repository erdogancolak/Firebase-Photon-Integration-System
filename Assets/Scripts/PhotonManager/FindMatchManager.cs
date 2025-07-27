using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FindMatchManager : MonoBehaviourPunCallbacks
{
    private string gameVersion = "1.0";
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private GameObject findMatchPanel;
    [SerializeField] private TMP_Text findMatchStatusText;
    [SerializeField] private TMP_Text findMatchTimerText;

    [Header("FindMatch System")]
    [SerializeField] private float fakeSearchTimer;
    [SerializeField] private Coroutine fakeTimerCoroutine;

    private bool isSearching = false;      
    private float timeAccumulator = 0f;    
    private int displaySeconds = 0;

    void Start()
    {
        findMatchPanel.SetActive(false);
        playButton.interactable = false;

        if(UserDataManager.instance != null && UserDataManager.instance.isDataLoaded)
        {
            PhotonNetwork.NickName = UserDataManager.instance.UserNickname;
        }
        else
        {
            PhotonNetwork.NickName = "Oyuncu " + Random.Range(1000, 9999);
            Debug.LogWarning("UserDataManager'dan isim alýnamadý, rastgele isim atandý.");
        }

        Debug.Log("Oyuncu Adi " + PhotonNetwork.NickName);
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;

        Debug.Log("Photon Sunucusuna Baðlanýyor...");
        PhotonNetwork.ConnectUsingSettings();
    }

    private void Update()
    {
        SearchTime();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Master Sunucusuna Baðlanýldý!!");

        playButton.interactable = true;
    }

    public void FindMatch()
    {
        playButton.gameObject.SetActive(false);
        findMatchPanel.SetActive(true);
        findMatchStatusText.text = "Oyun Araniyor...";

        isSearching = true;
        displaySeconds = 0;
        timeAccumulator = 0f;
        findMatchTimerText.text = "0";

        fakeTimerCoroutine = StartCoroutine(fakeFindMatchTimer());
    }
    IEnumerator fakeFindMatchTimer()
    {
        Debug.Log("Fake Timer Baþlatýldý... " + fakeSearchTimer.ToString());

        yield return new WaitForSeconds(fakeSearchTimer);

        Debug.Log("Fake Timer Doldu, Oda aranýyor...");

        if(PhotonNetwork.IsConnected)
        {
            Debug.Log("Rastgele bir odaya baðlanýyor...");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            findMatchStatusText.text = "Sunucu Hatasý...";
            Debug.LogWarning("Master Sunucusuna Baðlanýlamadý! Tekrar Deneniyor!");
            PhotonNetwork.ConnectUsingSettings();
        }
    }
    public void CancelMatchmaking()
    {
        if(fakeTimerCoroutine != null)
        {
            StopCoroutine(fakeTimerCoroutine);
            Debug.Log("Maç Arama Coroutine Ýptal Edildi!");
        }

        Debug.Log("Maç arama iptal edildi!");

        playButton.gameObject.SetActive(true);
        findMatchPanel.SetActive(false);

        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public override void OnJoinRandomFailed(short returnCode,string message)
    {
        Debug.Log("Rastgele bir oda bulunamadý! Yeni bir oda kuruluyor!");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4 });
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya baþarýyla katýlýndý! Oda Adý = " + PhotonNetwork.CurrentRoom.Name);

        if(PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Master Client Olarak girildi!");

            PhotonNetwork.LoadLevel("LobbyScene");
        }
        else
        {
            Debug.Log("Odaya Katýlýndý!");
        }
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Odadan Ayrýlýndý!");

        if(findMatchPanel.activeSelf)
        {
            playButton.gameObject.SetActive(true);
            findMatchPanel.SetActive(false);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Sunucu Baðlantýsý Kesildi! " + cause);

        if (findMatchPanel.activeSelf)
        {
            playButton.gameObject.SetActive(true);
            findMatchPanel.SetActive(false);
        }
        playButton.interactable = false;
    }
    void SearchTime()
    {
        if(!isSearching)
        {
            return;
        }

        timeAccumulator += Time.deltaTime;

        if(timeAccumulator >= 1f)
        {
            displaySeconds++;
            findMatchTimerText.text = displaySeconds.ToString();
            timeAccumulator -= 1f;
        }
    }
    
}
