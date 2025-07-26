using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Slider loadingSlider;

    [Header("Loading Settings")]
    [SerializeField] private float fakeLoadSpeed;
    void Start()
    {
        Debug.Log("Test");
        if(PlayerPrefs.GetInt("IsLoggedIn", 0) == 1)
        {
            StartCoroutine(LoadSceneAsync("ClientScene"));
        }
        else
        {
            StartCoroutine(LoadSceneAsync("LoginScene"));
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
        Debug.Log("Test2");
        operation.allowSceneActivation = true;
    }
}
