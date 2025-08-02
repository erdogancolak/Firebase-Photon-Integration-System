using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ExitGames.Client.Photon;

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

    void Start()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0;
        
        
        StartCoroutine(LoadGameSequence());
    }

    IEnumerator LoadGameSequence()
    {
        if(PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
        {
            if(UserDataManager.instance != null && !UserDataManager.instance.isDataLoaded)
            {
                yield return StartCoroutine(FetchUserDataCoroutine());
            }
        }

        yield return StartCoroutine(UpdateSlider(0.4f));

        PhotonNetwork.NickName = UserDataManager.instance != null && UserDataManager.instance.isDataLoaded
       ? UserDataManager.instance.UserNickname
       : "Oyuncu" + Random.Range(1000, 9999);

       Debug.Log("Photon Sunucusuna Baðlanýlýyor... " + PhotonNetwork.NickName);

        ConnectToPhoton();

        yield return new WaitUntil(() => isReadyToProceed);

        yield return StartCoroutine(UpdateSlider(0.8f));

        string sceneToLoad;
        string roomToRejoin = GameStateManager.LastRoomName;

        if(!string.IsNullOrEmpty(roomToRejoin))
        {
            Debug.Log($"Oyuncu bir odadan düþmüþ. Odaya tekrar giriliyor: {roomToRejoin}");
            PhotonNetwork.RejoinRoom(roomToRejoin);

            yield break;
        }
        
        if(PlayerPrefs.GetInt("IsLoggedIn", 0) == 0)
        {
            sceneToLoad = "LoginScene";
            Debug.Log("Giriþ yapýlmamýþ. LoginScene yükleniyor...");
        }
        else
        {
            sceneToLoad = GameStateManager.LastSceneName ?? "ClientScene";
            Debug.Log($"Giriþ yapýlmýþ. Son bilinen sahneye yönlendiriliyor: {sceneToLoad}");
        }

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (operation.progress < 0.9f)
        {
            float progress = 0.8f + (Mathf.Clamp01(operation.progress / 0.9f) * 0.2f);
            if (loadingSlider != null) loadingSlider.value = progress;
            yield return null;
        }

        if (loadingSlider != null) loadingSlider.value = 1f;
        operation.allowSceneActivation = true;
    }
    public void RestartLoadSequence()
    {
        Debug.Log("Yükleme süreci yeniden baþlatýlýyor...");

        StopAllCoroutines();

        if (loadingSlider != null)
            loadingSlider.value = 0;

        StartCoroutine(LoadGameSequence());
    }

    IEnumerator FetchUserDataCoroutine()
    {
        FirebaseUser user = FirebaseAuth.DefaultInstance.CurrentUser;
        if(user == null)
        {
            Debug.Log("Kullanýcý Bulunamadý!");
            yield break;
        }

        bool isFetchComplete = false;
        DocumentReference docRef = FirebaseFirestore.DefaultInstance.Collection("kullanicilar").Document(user.UserId);
        docRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted && !task.IsFaulted && task.Result.Exists)
            {
                var userData = task.Result.ToDictionary();
                int elo = System.Convert.ToInt32(userData["eloPoints"]);

                UserDataManager.instance.setUserData(
                    userData["nickname"].ToString(),
                    userData["email"].ToString(),
                    user.UserId,
                    elo);
            }
            isFetchComplete = true;
        });

        yield return new WaitUntil(() => isFetchComplete);
    }

    IEnumerator UpdateSlider(float targetValue)
    {
        float startValue = loadingSlider.value;
        float time = 0;

        while (time < 0.5f) 
        {
            loadingSlider.value = Mathf.Lerp(startValue, targetValue, time / 0.5f);
            time += Time.deltaTime;
            yield return null;
        }
        loadingSlider.value = targetValue;
    }

    void ConnectToPhoton()
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
