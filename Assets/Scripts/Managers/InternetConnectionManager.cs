using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InternetConnectionManager : MonoBehaviourPunCallbacks
{
    public static InternetConnectionManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject noInternetPanel;
    private GameObject activePanelInstance;

    [Header("Connection Settings")]
    [SerializeField] private float checkInterval;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        StartCoroutine(CheckInternetPeriodicallyCoroutine());
    }

    IEnumerator CheckInternetPeriodicallyCoroutine()
    {
        while (true)
        {
            CheckInternetConnection();

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void CheckInternetConnection()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if(activePanelInstance == null)
            {
                ShowNoInternetPanel();
            }
        }
    }

    void ShowNoInternetPanel()
    {
        if (noInternetPanel == null) return;

        GameStateManager.IsNoInternetPanelActive = true;

        activePanelInstance = Instantiate(noInternetPanel);

        Canvas sceneCanvas = FindAnyObjectByType<Canvas>();

        if (sceneCanvas != null)
        {
            activePanelInstance.transform.SetParent(sceneCanvas.transform, false);
        }
        else
        {
            Debug.Log("Canvas Bulunmadý!");
            Destroy(activePanelInstance);
            return;
        }

        var retryButton = activePanelInstance.transform.Find("RetryButton")?.GetComponent<Button>();
        var quitButton = activePanelInstance.transform.Find("QuitButton")?.GetComponent<Button>();

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryButton);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitButton);
    }
    public void RetryButton()
    {
        if(Application.internetReachability != NetworkReachability.NotReachable)
        {
            Debug.Log("Ýnternet baðlantýsý geri geldi. Yeniden baðlanýlýyor...");

            if (activePanelInstance != null)
            {
                Destroy(activePanelInstance);
                activePanelInstance = null;
            }
            GameStateManager.IsNoInternetPanelActive = false;
            if (SceneManager.GetActiveScene().name == "LoadingScene" && LoadingManager.Instance != null)
            {
                Debug.Log("LoadingScene aktif, yükleme süreci yeniden baþlatýlýyor.");
                LoadingManager.Instance.RestartLoadSequence();
            }
            else
            {
                if (RoomManager.instance != null)
                {
                    RoomManager.instance.ReconnectAndRejoin();
                }
                else
                {
                    if (!PhotonNetwork.IsConnected)
                    {
                        PhotonNetwork.ConnectUsingSettings();
                    }
                }
            }
        }
        else
        {
            Debug.LogWarning("Hala internet baðlantýsý yok. Tekrar denenemedi.");
        }
    }
    public void QuitButton()
    {
        Debug.Log("Oyundan çýkma butonuna basýldý. Panel kapatýlýyor ve oyun kapanýyor.");

        Destroy(activePanelInstance);
        activePanelInstance = null;

        Application.Quit();
    }
    
}
