using Firebase.Auth;
using Google;
using Photon.Pun;
using System.Threading.Tasks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserDataManager : MonoBehaviour
{
    public static UserDataManager instance { get; private set; }

    public string UserNickname { get; private set; }
    public string UserEmail { get; private set; }
    public string UserID { get; private set; }

    public bool isDataLoaded { get; private set; }
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            isDataLoaded = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void setUserData(string nickname,string email,string userId)
    {
        UserNickname = nickname;
        UserEmail = email;
        UserID = userId;
        isDataLoaded = true;
    }
    public void SignOut()
    {
        StartCoroutine(SignOutRoutine());
    }
    private IEnumerator SignOutRoutine()
    {
        Debug.Log("Çýkýþ yapma iþlemi baþlatýldý...");

        if (GoogleSignIn.DefaultInstance != null)
        {
            GoogleSignIn.DefaultInstance.SignOut();
            Debug.Log("Google oturumu kapatýldý.");
        }

        FirebaseAuth.DefaultInstance.SignOut();
        Debug.Log("Firebase oturumu kapatýldý.");

        if (PhotonNetwork.IsConnected)
        {
            PhotonNetwork.Disconnect();
            Debug.Log("Photon baðlantýsý kesildi.");
        }

        PlayerPrefs.DeleteKey("IsLoggedIn");
        Debug.Log("PlayerPrefs 'IsLoggedIn' anahtarý silindi.");

        this.UserNickname = null;
        this.UserEmail = null;
        this.UserID = null;
        this.isDataLoaded = false;
        Debug.Log("UserDataManager verileri sýfýrlandý.");

        GameStateManager.LastSceneName = "ClientScene";
        GameStateManager.LastRoomName = null;
        Debug.Log("GameStateManager durumu sýfýrlandý.");
 
        yield return new WaitForSeconds(0.5f);

        SceneManager.LoadScene("LoginScene");
    }
}
