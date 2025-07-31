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
            yield return null;
        }

        Debug.Log("ClientScene: Photon baðlantýsý hazýr. Nickname: " + PhotonNetwork.NickName);
        playButton.interactable = true;
    }

    public void FindMatch()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            Debug.LogError("Maç aranamýyor, sunucu baðlantýsý yok!");
            return;
        }

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
