using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    public static LoadingManager Instance { get; private set; }

    [SerializeField] private string gameVersion = "1.0";

    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [SerializeField] private float fakeLoadSpeed;

    private bool isReadyToProceed = false;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    async void Start()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0;


        await LoadGameSequence();
    }

    async Task LoadGameSequence()
    {
        string firebaseUserId = null;

        if(PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
        {
            Debug.Log("Giriþ yapýlmýþ, Firebase kullanýcýsý bekleniyor...");

            FirebaseUser user = await UserDataManager.instance.InitializeAndFetchUserAsync();

            if (user != null && UserDataManager.instance.isDataLoaded)
            {
                firebaseUserId = UserDataManager.instance.UserID;
                Debug.Log($"Kullanýcý verisi baþarýyla çekildi. UserID: {firebaseUserId}");
            }
            else
            {
                Debug.LogError("Kullanýcý verisi alýnamadý. Login ekranýna yönlendiriliyor.");
                PlayerPrefs.DeleteKey("IsLoggedIn");
                SceneManager.LoadScene("LoginScene");
                return; 
            }
        }
        await UpdateSlider(0.4f);

        PhotonNetwork.NickName = UserDataManager.instance != null && UserDataManager.instance.isDataLoaded
       ? UserDataManager.instance.UserNickname
       : "Oyuncu" + Random.Range(1000, 9999);

       Debug.Log("Photon Sunucusuna Baðlanýlýyor... " + PhotonNetwork.NickName);
        ConnectToPhoton(firebaseUserId);

        while (!isReadyToProceed)
        {
            await Task.Yield();
        }

        await UpdateSlider(0.8f);

        string roomToRejoin = GameStateManager.LastRoomName;
        if (!string.IsNullOrEmpty(roomToRejoin))
        {
            Debug.Log($"Oyuncu bir odadan düþmüþ. Odaya tekrar giriliyor: {roomToRejoin}");
            PhotonNetwork.RejoinRoom(roomToRejoin);
            return;
        }

        string sceneToLoad = (PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
            ? (GameStateManager.LastSceneName ?? "ClientScene")
            : "LoginScene";

        Debug.Log($"Sahne yükleniyor: {sceneToLoad}");

        await UpdateSlider(1f);
        Debug.Log($"Photon ile sahne yükleniyor: {sceneToLoad}");
        PhotonNetwork.LoadLevel(sceneToLoad);
    }
    public void RestartLoadSequence()
    {
        Debug.Log("Yükleme süreci yeniden baþlatýlýyor...");

        StopAllCoroutines();

        if (loadingSlider != null)
            loadingSlider.value = 0;

        _ = LoadGameSequence();
    }
    async Task UpdateSlider(float targetValue)
    {
        if (loadingSlider == null) return;

        float startValue = loadingSlider.value;
        float time = 0;
        float duration = 0.5f;

        while (time < duration) 
        {
            loadingSlider.value = Mathf.Lerp(startValue, targetValue, time / 0.5f);
            time += Time.deltaTime;
            await Task.Yield();
        }
        loadingSlider.value = targetValue;
    }

    void ConnectToPhoton(string userId)
    {
        isReadyToProceed = false;
        PhotonNetwork.AutomaticallySyncScene = true;

        if(PhotonNetwork.IsConnected)
        {
            SetPlayerEloProperty();
            PhotonNetwork.JoinLobby();
        }
        else
        {
            if(!string.IsNullOrEmpty(userId))
            {
                PhotonNetwork.AuthValues = new AuthenticationValues(userId);
                Debug.Log($"Photon AuthValues ayarlandý. UserID: {userId}");
            }
            else
            {
                Debug.LogError("Firebase UserID bulunamadý! Photon'a anonim olarak baðlanýlacak.");
            }

            PhotonNetwork.GameVersion = gameVersion;
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void SetPlayerEloProperty()
    {
        if (UserDataManager.instance != null && UserDataManager.instance.isDataLoaded)
        {
            var customProperties = new ExitGames.Client.Photon.Hashtable();
            customProperties["elo"] = UserDataManager.instance.EloPoints;
            PhotonNetwork.LocalPlayer.SetCustomProperties(customProperties);
            Debug.Log($"Oyuncunun Elo'su ({UserDataManager.instance.EloPoints}) aða bildirildi!");
        }
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("LoadingManager: Master sunucusuna baþarýyla baðlanýldý!");

        SetPlayerEloProperty();
        PhotonNetwork.JoinLobby();
    }
    public override void OnJoinedLobby()
    {
        Debug.Log("Lobiye girildi! Yükleme devam edebilir.");
        isReadyToProceed = true;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Baðlantý koptu: " + cause);
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("Odaya baþarýyla (yeniden) girildi. Oyun sahnesi yükleniyor...");

        SceneManager.LoadScene("LobbyScene");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.LogError($"Odaya yeniden girilemedi: {message}. Ana menüye gidiliyor.");
        GameStateManager.LastRoomName = null; 
        SceneManager.LoadScene("ClientScene");
    }
}
