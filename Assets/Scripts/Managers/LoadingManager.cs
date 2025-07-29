using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [SerializeField] private float fakeLoadSpeed;

    [Header("Connectivity")]
    [SerializeField] private GameObject noInternetPanel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button quitButton;

    [Header("Error Circle Defender")]
    public static int connectionRetries = 0;
    private const int MAX_RETRIES = 3;
    private bool isPhotonConnected;
    void Start()
    {
        if (loadingSlider != null)
            loadingSlider.value = 0;
        if(noInternetPanel != null)
            noInternetPanel.SetActive(false);
        if (retryButton != null)
            retryButton.onClick.AddListener(RetryConnection);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitApplication);

        InitiateLoading();
    }

    public void InitiateLoading()
    {
        if(connectionRetries >= MAX_RETRIES)
        {
            Debug.Log("Maksimum deneme sayýsýna ulaþýldý. Sunucuya baðlanýlamýyor.");

            if (noInternetPanel != null) 
                noInternetPanel.SetActive(true);

            Application.Quit();
        }
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Ýnternet baðlantýsý bulunamadý.");
            if (noInternetPanel != null)
                noInternetPanel.SetActive(true);
        }
        if (noInternetPanel != null)
            noInternetPanel.SetActive(false);

        StartCoroutine(LoadGameSequence());
    }

    IEnumerator LoadGameSequence()
    {
        if(PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
        {
            if(UserDataManager.instance != null || !UserDataManager.instance.isDataLoaded)
            {
                yield return StartCoroutine(FetchUserDataCoroutine());
            }
        }

        yield return StartCoroutine(UpdateSlider(0.4f));

        Debug.Log("Photon Sunucusuna Baðlanýlýyor...");
        ConnectToPhoton();

        yield return new WaitUntil(() => isPhotonConnected);

        yield return StartCoroutine(UpdateSlider(0.8f));

        string sceneToLoad = PlayerPrefs.GetInt("IsLoggedIn", 0) == 1 ? "ClientScene" : "LoginScene";
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while(operation.progress < 0.9f)
        {
            float progress = Mathf.Clamp01(operation.progress / .9f) * .2f;
            loadingSlider.value = progress;
            yield return null;
        }
        loadingSlider.value = 1f;

        operation.allowSceneActivation = true;
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
                UserDataManager.instance.setUserData(
                    userData["nickname"].ToString(),
                    userData["email"].ToString(),
                    user.UserId);
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
        if(PhotonNetwork.IsConnected)
        {
            isPhotonConnected = true;
            return;
        }

        if (UserDataManager.instance != null && UserDataManager.instance.isDataLoaded)
        {
            PhotonNetwork.NickName = UserDataManager.instance.UserNickname;
        }
        else
        {
            PhotonNetwork.NickName = "Oyuncu" + Random.Range(1000, 9999);
        }
        PhotonNetwork.GameVersion = "1.0";
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("LoadingManager: Master sunucusuna baþarýyla baðlanýldý!");
        isPhotonConnected = true;

        connectionRetries = 0;
    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Baðlantý koptu: " + cause);

        connectionRetries++;

        if (noInternetPanel != null) noInternetPanel.SetActive(true);
    }

    public void RetryConnection()
    {
        Debug.Log("Baðlantý yeniden deneniyor...");

        if (noInternetPanel != null) 
            noInternetPanel.SetActive(false);

        connectionRetries = 0;
        StartCoroutine(LoadGameSequence());
    }

    public void QuitApplication()
    {
        Debug.Log("Uygulamadan çýkýlýyor...");

        Application.Quit();
    }
}
