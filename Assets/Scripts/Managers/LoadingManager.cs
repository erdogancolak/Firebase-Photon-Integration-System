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
using WebSocketSharp;

public class LoadingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private string gameVersion = "1.0";
    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [SerializeField] private float fakeLoadSpeed;

    private bool isPhotonConnected;

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
            if(UserDataManager.instance != null || !UserDataManager.instance.isDataLoaded)
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

        yield return new WaitUntil(() => isPhotonConnected);

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
            sceneToLoad = GameStateManager.LastSceneName;
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
        
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    public override void OnConnectedToMaster()
    {
        Debug.Log("LoadingManager: Master sunucusuna baþarýyla baðlanýldý!");
        isPhotonConnected = true;

    }
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError("Baðlantý koptu: " + cause);
    }

    public void RetryConnection()
    {
        Debug.Log("Baðlantý yeniden deneniyor...");

        StartCoroutine(LoadGameSequence());
    }

    public void QuitApplication()
    {
        Debug.Log("Uygulamadan çýkýlýyor...");

        Application.Quit();
    }
}
