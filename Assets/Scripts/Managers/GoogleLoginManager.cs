using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Google;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Globalization;

public class GoogleLoginManager : MonoBehaviour
{
    [Header("Google API")]
    // Web Client ID'nizi buraya girin
    private string GoogleAPI = "403346372927-5enk77qjek6j1maje1toq6010umug62v.apps.googleusercontent.com";
    private GoogleSignInConfiguration configuration;

    [Header("Firebase Auth")]
    private FirebaseAuth auth;
    private FirebaseUser user;
    private FirebaseFirestore db;

    [Header("UI References")]
    [SerializeField] private GameObject LoginPanel;
    [SerializeField] private GameObject UserPanel;
    [SerializeField] private GameObject nicknamePanel;
    [SerializeField] private TMP_InputField nicknameInput;
    [SerializeField] private Button nicknameSubmit;
    [SerializeField] private TMP_Text UserEmail;
    [SerializeField] private TMP_Text Username;
    [SerializeField] private Image UserProfilePic;


    private string imageUrl;
    private bool isGoogleSignInInitialized = false;

    private void Start()
    {
        InitFirebase();
    }

    void InitFirebase()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;

        auth.StateChanged += authStateChanged;
        authStateChanged(this, null);
    }

    void authStateChanged(object sender,System.EventArgs eventArgs)
    {
        if(auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
            if(signedIn && user != null)
            {
                Debug.Log("Oturum Kapatýldý");
            }
            user = auth.CurrentUser;
            if(signedIn)
            {
                Debug.Log($"Oturum Açýldý : {user.Email}");
                CheckUserInDatabase(user);
            }
        }
    }

    void CheckUserInDatabase(FirebaseUser newUser)
    {
        DocumentReference userDocRef = db.Collection("kullanicilar").Document(newUser.UserId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("Firestore'dan veri alýnamadý: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log($"Hoþ geldin {snapshot.GetValue<string>("nickname")}! Oyun sahnesi yükleniyor...");

                var userData = snapshot.ToDictionary();
                UserDataManager.instance.setUserData(userData["nickname"].ToString(), userData["email"].ToString(), newUser.UserId);

                PlayerPrefs.SetInt("IsLoggedIn", 1);
                PlayerPrefs.Save();

                SceneManager.LoadScene("ClientScene");
            }
            else
            {
                Debug.Log("Yeni kullanýcý tespit edildi. Nickname paneli açýlýyor.");
                LoginPanel.SetActive(false);
                UserPanel.SetActive(false);
                nicknamePanel.SetActive(true);

                nicknameSubmit.onClick.RemoveAllListeners();
                nicknameSubmit.onClick.AddListener(() =>
                {
                    SaveNickname(newUser);
                });
            }
        });
    }
    public void SaveNickname(FirebaseUser userToSave)
    {
        string nickname = nicknameInput.text;
        if(string.IsNullOrEmpty(nickname))
        {
            Debug.LogError("Nickname Boþ Olamaz");
            return;
        }
        DocumentReference userDocRef = db.Collection("kullanicilar").Document(userToSave.UserId);

        var userData = new Dictionary<string, object>
        {
            {"email" , userToSave.Email},
            {"nickname" , nickname },
            {"created_at", FieldValue.ServerTimestamp },
            {"device_language",Application.systemLanguage.ToString() },
            {"country_code",RegionInfo.CurrentRegion.TwoLetterISORegionName },
            {"device_model",SystemInfo.deviceModel }
        };

        userDocRef.SetAsync(userData).ContinueWithOnMainThread(task => 
        {
            if(task.IsFaulted)
            {
                Debug.LogError("Nickname kaydedilemedi: " + task.Exception);
                return;
            }

            Debug.Log("Nickname baþarýyla kaydedildi! Oyun sahnesi yükleniyor...");
            nicknamePanel.SetActive(false);

            UserDataManager.instance.setUserData(nickname,userToSave.Email, userToSave.UserId);

            PlayerPrefs.SetInt("IsLoggedIn", 1);
            PlayerPrefs.Save();

            SceneManager.LoadScene("ClientScene");
        });
    }

    public void Login()
    {
        if (!isGoogleSignInInitialized)
        {
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                WebClientId = GoogleAPI,
                RequestEmail = true
            };
            isGoogleSignInInitialized = true;
        }

        GoogleSignIn.DefaultInstance.SignIn().ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogWarning("Google sign-in was canceled.");
                return;
            }

            if (task.IsFaulted)
            {
                Debug.LogError("Google sign-in encountered an error: " + task.Exception);
                return;
            }

            GoogleSignInUser googleUser = task.Result;
            Credential credential = GoogleAuthProvider.GetCredential(googleUser.IdToken, null);

            auth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(authTask =>
            {
                if (authTask.IsCanceled)
                {
                    Debug.LogWarning("Firebase auth was canceled.");
                    return;
                }

                if (authTask.IsFaulted)
                {
                    Debug.LogError("Firebase auth failed: " + authTask.Exception);
                    return;
                }
                //user = authTask.Result;

                //Username.text = user.DisplayName;
                //UserEmail.text = user.Email;

                //LoginPanel.SetActive(false);
                //UserPanel.SetActive(true);

                //StartCoroutine(LoadImage(CheckImageUrl(user.PhotoUrl?.ToString())));
            });
        });
    }

    private string CheckImageUrl(string url)
    {
        if (!string.IsNullOrEmpty(url))
        {
            return url;
        }
        return imageUrl;
    }

    IEnumerator LoadImage(string imageUri)
    {
        if (string.IsNullOrEmpty(imageUri)) yield break; // URL boþsa iþlemi bitir

        using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUri))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(www);
                if (UserProfilePic != null)
                {
                    UserProfilePic.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    Debug.Log("Image loaded successfully.");
                }
            }
            else
            {
                Debug.LogError("Error loading profile image: " + www.error);
            }
        }
    }

    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
        }
        GoogleSignIn.DefaultInstance.SignOut();

        PlayerPrefs.SetInt("IsLoggedIn", 0);
        PlayerPrefs.Save();
        
        LoginPanel.SetActive(true);
        UserPanel.SetActive(false);
        nicknamePanel.SetActive(false);
    }
}
