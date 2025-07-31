//using Photon.Pun;
//using Photon.Realtime;
//using UnityEngine;
//using UnityEngine.SceneManagement;

//public class InternetConnectionManager : MonoBehaviourPunCallbacks
//{
//    public static InternetConnectionManager instance { get; private set; }

//    [Header("References")]
//    [SerializeField] private GameObject noInternetPanel;

//    private static string lastSceneBeforeDisconnect;
//    private static string lastRoomName;

//    private void Awake()
//    {
//        if(instance == null)
//        {
//            instance = this;
//            DontDestroyOnLoad(gameObject);
//        }
//        else if (instance != this)
//        {
//            Destroy(gameObject);
//        }
//    }

//    private void Update()
//    {
//        if(Application.internetReachability == NetworkReachability.NotReachable)
//        {
//            if(noInternetPanel == null)
//            {
//                ShowNoInternetPanel();
//            }
//        }
//    }
//    public override void OnDisconnected(DisconnectCause cause)
//    {
//        base.OnDisconnected(cause);

//        if(Application.internetReachability != NetworkReachability.NotReachable)
//        {
//            Debug.LogError("Photon baðlantýsý koptu: " + cause);
//            ShowNoInternetPanel();
//        }
//    }
//    void ShowNoInternetPanel()
//    {
//        if (noInternetPanel != null) return;

//        noInternetPanel = Instantiate(noInternetPanel);

//        var retryButton = noInternetPanel.transform.Find("RetryButton").GetComponent<UnityEngine.UI.Button>();
//        var quitButton = noInternetPanel.transform.Find("QuitButton").GetComponent<UnityEngine.UI.Button>();

//        retryButton.onClick.AddListener(OnRetryButtonClicked);
//        quitButton.onClick.AddListener(() => { Application.Quit(); });
//    }

//    public void OnRetryButtonClicked()
//    {
//        Debug.Log("Baðlantý yeniden denenecek. Son bilinen sahne: " + lastSceneBeforeDisconnect);

//        if(noInternetPanel != null)
//        {
//            Destroy(noInternetPanel);
//        }
//        SceneManager.LoadScene("LoadingScene");
//    }

//    public static void UpdatePlayerLocation(string sceneName, string roomName = null)
//    {
//        if (sceneName == "LobbyScene" || sceneName == "GameScene" || sceneName == "ClientScene")
//        {
//            lastSceneBeforeDisconnect = sceneName;
//            lastRoomName = roomName;
//            Debug.Log($"Oyuncu konumu güncellendi: Sahne={lastSceneBeforeDisconnect}, Oda={lastRoomName}");
//        }
//    }

//    public static string GetReturnSceneName()
//    {
//        return string.IsNullOrEmpty(lastSceneBeforeDisconnect) ? "ClientScene" : lastSceneBeforeDisconnect;
//    }

//    public static string GetReturnRoomName()
//    {
//        return lastRoomName;
//    }
//}
