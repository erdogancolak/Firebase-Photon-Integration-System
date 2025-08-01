using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InternetConnectionManager : MonoBehaviourPunCallbacks
{
    public static InternetConnectionManager instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject noInternetPanel;
    private GameObject activePanelInstance;

    private static string lastSceneBeforeDisconnect;
    private static string lastRoomName;

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
    
    private void Update()
    {
        CheckInternetConnection();
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
        }

        var retryButton = activePanelInstance.transform.Find("RetryButton").GetComponent<UnityEngine.UI.Button>();
        var quitButton = activePanelInstance.transform.Find("QuitButton").GetComponent<UnityEngine.UI.Button>();

        if (retryButton != null)
            retryButton.onClick.AddListener(RetryButton);
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitButton);
    }
    public void RetryButton()
    {
        Debug.Log("Tekrar deneme butonuna basýldý. Panel kapatýlýyor.");

        Destroy(activePanelInstance);

        activePanelInstance = null;

        SceneManager.LoadScene("LoadingScene");
    }
    public void QuitButton()
    {
        Debug.Log("Oyundan çýkma butonuna basýldý. Panel kapatýlýyor ve oyun kapanýyor.");

        Destroy(activePanelInstance);
        activePanelInstance = null;

        Application.Quit();
    }
    
}
