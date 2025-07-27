using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [SerializeField] private float fakeLoadSpeed;

    [Header("Connectivity")]
    [SerializeField] private GameObject noInternetPanel;
    [SerializeField] private Button retryButton;

    private bool isLoading = false;
    void Start()
    {
        if(noInternetPanel != null)
        {
            noInternetPanel.SetActive(false);
        }

        if (retryButton != null)
        {
            retryButton.onClick.AddListener(InitiateLoading);
        }

        InitiateLoading();
    }

    public void InitiateLoading()
    {
        if (isLoading) return;

        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogError("Ýnternet baðlantýsý bulunamadý.");
            if (noInternetPanel != null)
            {
                noInternetPanel.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Ýnternet baðlantýsý var. Yükleme baþlýyor.");
            isLoading = true; 
            if (noInternetPanel != null)
            {
                noInternetPanel.SetActive(false); 
            }

            if (PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
            {
                StartCoroutine(LoadSceneAsync("ClientScene"));
            }
            else
            {
                StartCoroutine(LoadSceneAsync("LoginScene"));
            }
        }
    }
    IEnumerator LoadSceneAsync(string sceneName)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        operation.allowSceneActivation = false;

        float visualProgress = 0f;

        while (visualProgress < 1f)
        {
            visualProgress += Time.deltaTime * fakeLoadSpeed;
            loadingSlider.value = Mathf.Clamp01(visualProgress);
            yield return null;
        }

        while(operation.progress < 0.9f)
        {
            yield return null;
        }
        operation.allowSceneActivation = true;
    }
}
