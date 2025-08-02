using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class FindMatchManager : MonoBehaviourPunCallbacks
{
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

    private bool isConnecting = false;

    void Start()
    {
        StartCoroutine(EnsureConnectedAndReady());
    }

    private void Update()
    {
        SearchTime();
    }
    IEnumerator EnsureConnectedAndReady()
    {
        playButton.interactable = false;
        findMatchPanel.SetActive(false);

        while(!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("ClientScene: Master sunucusuna durumun güncellenmesi bekleniyor...");
            if(!PhotonNetwork.IsConnected)
            {
                PhotonNetwork.ConnectUsingSettings();
            }
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("ClientScene: Photon baðlantýsý hazýr. Nickname: " + PhotonNetwork.NickName);
        playButton.interactable = true;
    }

    public void FindMatch()
    {
        if (isConnecting || isSearching) return;

        if(!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Maç aranamýyor, sunucu baðlantýsý yok! Yeniden baðlanýlýyor...");
            isConnecting = true;

            playButton.interactable = false;
            findMatchPanel.SetActive(true);
            findMatchStatusText.text = "Sunucuya Baðlanýlýyor...";
            findMatchTimerText.text = "";

            PhotonNetwork.ConnectUsingSettings();
            return;
        }
        isSearching = true;

        playButton.interactable = false;
        findMatchPanel.SetActive(true);
        findMatchStatusText.text = "Oyun Aranýyor...";

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

        PhotonNetwork.JoinRandomRoom();
    }
    public void CancelMatchmaking()
    {
        if(fakeTimerCoroutine != null)
        {
            StopCoroutine(fakeTimerCoroutine);
            Debug.Log("Maç Arama Coroutine Ýptal Edildi!");
        }

        isSearching = false;
        Debug.Log("Maç arama iptal edildi!");

        playButton.interactable = true;
        findMatchPanel.SetActive(false);

        if(PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("Master sunucusuna baþarýyla baðlanýldý!");
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye baþarýyla girildi.");
        playButton.interactable = true; 

        if (isConnecting)
        {
            isConnecting = false;
            Debug.Log("Yeniden baðlanma baþarýlý! Maç arama yeniden baþlatýlýyor...");
            FindMatch(); 
        }
    }


    public override void OnJoinRandomFailed(short returnCode,string message)
    {
        Debug.Log("Rastgele bir oda bulunamadý! Yeni bir oda kuruluyor!");

        PhotonNetwork.CreateRoom(null, new RoomOptions { MaxPlayers = 4,IsVisible = true,IsOpen = true });
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
            playButton.interactable = true;
            findMatchPanel.SetActive(false);
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if(GameStateManager.IsSigningOut)
        {
            Debug.Log("Kasýtlý çýkýþ yapýldý, otomatik yeniden baðlanma pasif.");
            return;
        }

        Debug.Log("Sunucu Baðlantýsý Kesildi! " + cause);

        isConnecting = false;
        isSearching = false;

        if(findMatchPanel.activeSelf)
        {
            playButton.interactable = true;
            findMatchPanel.SetActive(false);
        }
        playButton.interactable = false;

        StartCoroutine(EnsureConnectedAndReady());
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
